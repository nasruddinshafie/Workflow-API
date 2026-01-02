import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { PendingLeaveRequest } from '../types';
import { leaveService } from '../services/leaveService';
import './PendingApprovals.css';

const PendingApprovals: React.FC = () => {
  const { currentUser } = useAuth();
  const [pendingLeaves, setPendingLeaves] = useState<PendingLeaveRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [selectedLeave, setSelectedLeave] = useState<PendingLeaveRequest | null>(null);
  const [expandedLeave, setExpandedLeave] = useState<string | null>(null);
  const [selectedCommand, setSelectedCommand] = useState<{ command: string; localizedName: string } | null>(null);
  const [showCommandModal, setShowCommandModal] = useState(false);

  useEffect(() => {
    if (currentUser) {
      loadPendingApprovals();
    }
  }, [currentUser]);

  const loadPendingApprovals = async () => {
    if (!currentUser) return;

    setLoading(true);
    try {
      const leaves = await leaveService.getPendingApprovals(currentUser.id);
      setPendingLeaves(leaves);
    } catch (error: any) {
      console.error('Failed to load pending approvals:', error);
      setMessage({
        type: 'error',
        text: 'Failed to load pending approvals',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleExecuteCommand = async () => {
    if (!currentUser || !selectedLeave || !selectedCommand) return;

    setActionLoading(selectedLeave.workflowProcessId);
    setMessage(null);

    try {
      await leaveService.executeCommand(
        selectedLeave.workflowProcessId,
        selectedCommand.command,
        currentUser.id
      );

      setMessage({
        type: 'success',
        text: `Command '${selectedCommand.localizedName}' executed successfully!`,
      });

      setShowCommandModal(false);
      setSelectedCommand(null);
      setSelectedLeave(null);

      // Reload pending approvals
      await loadPendingApprovals();
    } catch (error: any) {
      setMessage({
        type: 'error',
        text: error.response?.data?.message || `Failed to execute command '${selectedCommand.localizedName}'`,
      });
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  };

  const calculateDaysBetween = (start: string, end: string) => {
    const startDate = new Date(start);
    const endDate = new Date(end);
    const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    return diffDays;
  };

  if (loading) {
    return (
      <div className="pending-approvals">
        <h2>Pending Approvals</h2>
        <div className="loading-container">
          <p>Loading pending approvals...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="pending-approvals">
      <div className="approvals-header">
        <h2>Pending Approvals</h2>
        <span className="pending-count">{pendingLeaves.length} pending</span>
      </div>

      {message && (
        <div className={`message ${message.type}`}>
          {message.text}
        </div>
      )}

      {pendingLeaves.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">✓</div>
          <h3>No Pending Approvals</h3>
          <p>You don't have any leave requests waiting for your approval.</p>
        </div>
      ) : (
        <div className="approvals-list">
          {pendingLeaves.map((leave) => (
            <div key={leave.leaveRequestId} className="approval-card">
              <div className="card-header">
                <div className="employee-info">
                  <h3>{leave.employeeName}</h3>
                  <span className="department">{leave.employeeDepartment}</span>
                </div>
                <span className={`status-badge ${leave.status.toLowerCase()}`}>
                  {leave.status}
                </span>
              </div>

              <div className="card-body">
                <div className="leave-details">
                  <div className="detail-row">
                    <span className="label">Leave Type:</span>
                    <span className="value">
                      {leave.leaveTypeColor && (
                        <span
                          className="leave-color-indicator"
                          style={{ backgroundColor: leave.leaveTypeColor }}
                        />
                      )}
                      {leave.leaveType}
                    </span>
                  </div>
                  <div className="detail-row">
                    <span className="label">Duration:</span>
                    <span className="value">
                      {formatDate(leave.startDate)} - {formatDate(leave.endDate)}
                      <span className="days-badge">{leave.totalDays} days</span>
                    </span>
                  </div>
                  <div className="detail-row">
                    <span className="label">Submitted:</span>
                    <span className="value">{formatDate(leave.submittedDate)}</span>
                  </div>

                  {/* Expandable Details */}
                  {expandedLeave === leave.leaveRequestId && (
                    <>
                      <div className="detail-row">
                        <span className="label">Email:</span>
                        <span className="value">{leave.employeeEmail}</span>
                      </div>
                      {leave.balance && (
                        <div className="detail-row">
                          <span className="label">Leave Balance:</span>
                          <div className="balance-breakdown">
                            <div className="balance-item">
                              <span className="balance-label">Total:</span>
                              <span className="balance-value">{leave.balance.totalDays}</span>
                            </div>
                            <div className="balance-item">
                              <span className="balance-label">Used:</span>
                              <span className="balance-value">{leave.balance.usedDays}</span>
                            </div>
                            <div className="balance-item">
                              <span className="balance-label">Pending:</span>
                              <span className="balance-value">{leave.balance.pendingDays}</span>
                            </div>
                            <div className="balance-item available">
                              <span className="balance-label">Available:</span>
                              <span className="balance-value">{leave.balance.availableDays}</span>
                            </div>
                          </div>
                        </div>
                      )}
                      {leave.balance && leave.totalDays > leave.balance.availableDays && (
                        <div className="warning-message">
                          ⚠️ This request exceeds available balance by{' '}
                          {(leave.totalDays - leave.balance.availableDays).toFixed(1)} days
                        </div>
                      )}
                    </>
                  )}

                  <div className="detail-row full-width">
                    <span className="label">Reason:</span>
                    <p className="reason-text">{leave.reason}</p>
                  </div>

                  <button
                    className="expand-btn"
                    onClick={() =>
                      setExpandedLeave(
                        expandedLeave === leave.leaveRequestId ? null : leave.leaveRequestId
                      )
                    }
                  >
                    {expandedLeave === leave.leaveRequestId ? '▲ Show Less' : '▼ Show More Details'}
                  </button>
                </div>
              </div>

              {/* Available Commands */}
              {leave.availableCommands && leave.availableCommands.length > 0 && (
                <div className="card-actions">
                  {leave.availableCommands.map((cmd, idx) => (
                    <button
                      key={idx}
                      className="command-button"
                      onClick={() => {
                        setSelectedLeave(leave);
                        setSelectedCommand({
                          command: cmd.commandName,
                          localizedName: cmd.localizedName || cmd.commandName
                        });
                        setShowCommandModal(true);
                      }}
                      disabled={actionLoading === leave.workflowProcessId}
                    >
                      {actionLoading === leave.workflowProcessId ? 'Processing...' : (cmd.localizedName || cmd.commandName)}
                    </button>
                  ))}
                </div>
              )}
              {(!leave.availableCommands || leave.availableCommands.length === 0) && (
                <div className="card-actions">
                  <p className="no-commands-message">No actions available</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {showCommandModal && selectedLeave && selectedCommand && (
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
                <p><strong>Employee:</strong> {selectedLeave.employeeName}</p>
                <p><strong>Leave Type:</strong> {selectedLeave.leaveType}</p>
                <p><strong>Duration:</strong> {formatDate(selectedLeave.startDate)} - {formatDate(selectedLeave.endDate)} ({selectedLeave.totalDays} days)</p>
                <p><strong>Status:</strong> {selectedLeave.status}</p>
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

export default PendingApprovals;
