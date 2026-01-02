import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { MyLeaveRequest } from '../types';
import { leaveService } from '../services/leaveService';
import './MyLeaveRequests.css';

const MyLeaveRequests: React.FC = () => {
  const { currentUser } = useAuth();
  const [myRequests, setMyRequests] = useState<MyLeaveRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [expandedRequest, setExpandedRequest] = useState<string | null>(null);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<MyLeaveRequest | null>(null);
  const [selectedCommand, setSelectedCommand] = useState<{ command: string; localizedName: string } | null>(null);
  const [showCommandModal, setShowCommandModal] = useState(false);

  useEffect(() => {
    if (currentUser) {
      loadMyRequests();
    }
  }, [currentUser]);

  const loadMyRequests = async () => {
    if (!currentUser) return;

    setLoading(true);
    try {
      const requests = await leaveService.getMyLeaveRequests(currentUser.id);
      setMyRequests(requests);
    } catch (error: any) {
      console.error('Failed to load leave requests:', error);
      setMessage({
        type: 'error',
        text: 'Failed to load leave requests',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleExecuteCommand = async () => {
    if (!currentUser || !selectedRequest || !selectedCommand) return;

    setActionLoading(selectedRequest.leaveRequestId);
    setMessage(null);

    try {
      await leaveService.executeCommand(
        selectedRequest.workflowProcessId,
        selectedCommand.command,
        currentUser.id
      );

      setMessage({
        type: 'success',
        text: `Command '${selectedCommand.localizedName}' executed successfully!`,
      });

      setShowCommandModal(false);
      setSelectedCommand(null);
      setSelectedRequest(null);

      // Reload requests
      await loadMyRequests();
    } catch (error: any) {
      setMessage({
        type: 'error',
        text: error.response?.data?.message || `Failed to execute command '${selectedCommand.localizedName}'`,
      });
    } finally {
      setActionLoading(null);
    }
  };

  const handleCancelRequest = async () => {
    if (!currentUser || !selectedRequest) return;

    setActionLoading(selectedRequest.leaveRequestId);
    setMessage(null);

    try {
      await leaveService.cancelLeave(selectedRequest.leaveRequestId, currentUser.id);

      setMessage({
        type: 'success',
        text: 'Leave request cancelled successfully!',
      });

      setShowCancelModal(false);
      setSelectedRequest(null);

      // Reload requests
      await loadMyRequests();
    } catch (error: any) {
      setMessage({
        type: 'error',
        text: error.response?.data?.message || 'Failed to cancel leave request',
      });
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'status-pending';
      case 'approved':
      case 'managerapproved':
        return 'status-approved';
      case 'rejected':
        return 'status-rejected';
      case 'cancelled':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  };

  const canCancelRequest = (request: MyLeaveRequest) => {
    return request.availableCommands?.some(cmd =>
      cmd.commandName.toLowerCase() === 'cancel'
    ) ?? false;
  };

  if (loading) {
    return (
      <div className="my-leave-requests">
        <h2>My Leave Requests</h2>
        <div className="loading-container">
          <p>Loading your leave requests...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="my-leave-requests">
      <div className="requests-header">
        <h2>My Leave Requests</h2>
        <span className="requests-count">{myRequests.length} requests</span>
      </div>

      {message && (
        <div className={`message ${message.type}`}>
          {message.text}
        </div>
      )}

      {myRequests.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">📋</div>
          <h3>No Leave Requests</h3>
          <p>You haven't submitted any leave requests yet.</p>
        </div>
      ) : (
        <div className="requests-list">
          {myRequests.map((request) => (
            <div key={request.leaveRequestId} className="request-card">
              <div className="card-header">
                <div className="leave-type-info">
                  {request.leaveTypeColor && (
                    <span
                      className="leave-color-indicator"
                      style={{ backgroundColor: request.leaveTypeColor }}
                    />
                  )}
                  <h3>{request.leaveType}</h3>
                </div>
                <span className={`status-badge ${getStatusColor(request.status)}`}>
                  {request.status}
                </span>
              </div>

              <div className="card-body">
                <div className="leave-details">
                  <div className="detail-row">
                    <span className="label">Duration:</span>
                    <span className="value">
                      {formatDate(request.startDate)} - {formatDate(request.endDate)}
                      <span className="days-badge">{request.totalDays} days</span>
                    </span>
                  </div>
                  <div className="detail-row">
                    <span className="label">Submitted:</span>
                    <span className="value">{formatDate(request.submittedDate)}</span>
                  </div>

                  {/* Expandable Details */}
                  {expandedRequest === request.leaveRequestId && (
                    <>
                      <div className="detail-row">
                        <span className="label">Workflow ID:</span>
                        <span className="value workflow-id">{request.workflowProcessId}</span>
                      </div>
                      {request.balance && (
                        <div className="detail-row">
                          <span className="label">Leave Balance:</span>
                          <div className="balance-breakdown">
                            <div className="balance-item">
                              <span className="balance-label">Total:</span>
                              <span className="balance-value">{request.balance.totalDays}</span>
                            </div>
                            <div className="balance-item">
                              <span className="balance-label">Used:</span>
                              <span className="balance-value">{request.balance.usedDays}</span>
                            </div>
                            <div className="balance-item">
                              <span className="balance-label">Pending:</span>
                              <span className="balance-value">{request.balance.pendingDays}</span>
                            </div>
                            <div className="balance-item available">
                              <span className="balance-label">Available:</span>
                              <span className="balance-value">{request.balance.availableDays}</span>
                            </div>
                          </div>
                        </div>
                      )}
                      {request.availableCommands && request.availableCommands.length > 0 && (
                        <div className="detail-row">
                          <span className="label">Available Actions:</span>
                          <div className="available-commands">
                            {request.availableCommands.map((cmd, idx) => (
                              <button
                                key={idx}
                                className="command-button"
                                onClick={() => {
                                  setSelectedRequest(request);
                                  setSelectedCommand({
                                    command: cmd.commandName,
                                    localizedName: cmd.localizedName || cmd.commandName
                                  });
                                  setShowCommandModal(true);
                                }}
                                disabled={actionLoading === request.leaveRequestId}
                              >
                                {cmd.localizedName || cmd.commandName}
                              </button>
                            ))}
                          </div>
                        </div>
                      )}
                    </>
                  )}

                  <div className="detail-row full-width">
                    <span className="label">Reason:</span>
                    <p className="reason-text">{request.reason}</p>
                  </div>

                  <button
                    className="expand-btn"
                    onClick={() =>
                      setExpandedRequest(
                        expandedRequest === request.leaveRequestId ? null : request.leaveRequestId
                      )
                    }
                  >
                    {expandedRequest === request.leaveRequestId ? '▲ Show Less' : '▼ Show More Details'}
                  </button>
                </div>
              </div>

              {canCancelRequest(request) && request.status.toLowerCase() === 'pending' && (
                <div className="card-actions">
                  <button
                    className="cancel-btn"
                    onClick={() => {
                      setSelectedRequest(request);
                      setShowCancelModal(true);
                    }}
                    disabled={actionLoading === request.leaveRequestId}
                  >
                    {actionLoading === request.leaveRequestId ? 'Cancelling...' : 'Cancel Request'}
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {showCancelModal && selectedRequest && (
        <div className="modal-overlay" onClick={() => setShowCancelModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Cancel Leave Request</h3>
              <button className="close-btn" onClick={() => setShowCancelModal(false)}>
                ×
              </button>
            </div>
            <div className="modal-body">
              <p className="confirm-text">
                Are you sure you want to cancel this leave request?
              </p>
              <div className="modal-leave-info">
                <p><strong>Leave Type:</strong> {selectedRequest.leaveType}</p>
                <p><strong>Duration:</strong> {formatDate(selectedRequest.startDate)} - {formatDate(selectedRequest.endDate)} ({selectedRequest.totalDays} days)</p>
                <p><strong>Reason:</strong> {selectedRequest.reason}</p>
              </div>
            </div>
            <div className="modal-actions">
              <button
                className="modal-back-btn"
                onClick={() => setShowCancelModal(false)}
                disabled={!!actionLoading}
              >
                Go Back
              </button>
              <button
                className="modal-cancel-btn"
                onClick={handleCancelRequest}
                disabled={!!actionLoading}
              >
                {actionLoading ? 'Cancelling...' : 'Yes, Cancel Request'}
              </button>
            </div>
          </div>
        </div>
      )}

      {showCommandModal && selectedRequest && selectedCommand && (
        <div className="modal-overlay" onClick={() => setShowCommandModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Execute Command</h3>
              <button className="close-btn" onClick={() => setShowCommandModal(false)}>
                ×
              </button>
            </div>
            <div className="modal-body">
              <p className="confirm-text">
                Are you sure you want to execute the command "{selectedCommand.localizedName}"?
              </p>
              <div className="modal-leave-info">
                <p><strong>Leave Type:</strong> {selectedRequest.leaveType}</p>
                <p><strong>Duration:</strong> {formatDate(selectedRequest.startDate)} - {formatDate(selectedRequest.endDate)} ({selectedRequest.totalDays} days)</p>
                <p><strong>Status:</strong> {selectedRequest.status}</p>
              </div>
            </div>
            <div className="modal-actions">
              <button
                className="modal-back-btn"
                onClick={() => setShowCommandModal(false)}
                disabled={!!actionLoading}
              >
                Cancel
              </button>
              <button
                className="modal-confirm-btn"
                onClick={handleExecuteCommand}
                disabled={!!actionLoading}
              >
                {actionLoading ? 'Executing...' : `Execute ${selectedCommand.localizedName}`}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyLeaveRequests;
