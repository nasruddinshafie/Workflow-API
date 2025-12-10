import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { LeaveType } from '../types';
import { leaveService } from '../services/leaveService';
import { leaveBalanceService } from '../services/leaveBalanceService';
import './LeaveRequestForm.css';

const LeaveRequestForm: React.FC = () => {
  const { currentUser } = useAuth();
  const [leaveTypes, setLeaveTypes] = useState<LeaveType[]>([]);
  const [formData, setFormData] = useState({
    leaveType: '',
    startDate: '',
    endDate: '',
    reason: '',
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(
    null
  );
  const [availableDays, setAvailableDays] = useState<number | null>(null);
  const [manager, setManager] = useState<any>(null);
  const [loadingManager, setLoadingManager] = useState(false);

  useEffect(() => {
    leaveService
      .getAllLeaveTypes()
      .then((types) => {
        setLeaveTypes(types);
      })
      .catch((error) => {
        console.error('Failed to load leave types:', error);
      });
  }, []);

  useEffect(() => {
    if (currentUser) {
      setLoadingManager(true);
      leaveService
        .getUserManager(currentUser.id)
        .then((managerData) => {
          setManager(managerData);
        })
        .catch((error) => {
          console.error('Failed to load manager:', error);
          setMessage({
            type: 'error',
            text: 'Unable to load your manager. Please contact HR.',
          });
        })
        .finally(() => {
          setLoadingManager(false);
        });
    }
  }, [currentUser]);

  useEffect(() => {
    if (currentUser && formData.leaveType) {
      leaveBalanceService
        .getBalance(currentUser.id, formData.leaveType)
        .then((balance) => {
          setAvailableDays(balance.availableDays);
        })
        .catch((error) => {
          console.error('Failed to load balance:', error);
          setAvailableDays(null);
        });
    }
  }, [currentUser, formData.leaveType]);

  const calculateDays = (): number => {
    if (!formData.startDate || !formData.endDate) return 0;
    const start = new Date(formData.startDate);
    const end = new Date(formData.endDate);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    return diffDays;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!currentUser) return;

    if (!manager) {
      setMessage({
        type: 'error',
        text: 'You must have a manager assigned to submit a leave request. Please contact HR.',
      });
      return;
    }

    const daysRequired = calculateDays();

    if (availableDays !== null && daysRequired > availableDays) {
      setMessage({
        type: 'error',
        text: `Insufficient leave balance. You have ${availableDays} days available but requested ${daysRequired} days.`,
      });
      return;
    }

    setLoading(true);
    setMessage(null);

    try {
      const request = {
        employeeId: currentUser.id,
        employeeName: currentUser.fullName,
        startDate: formData.startDate,
        endDate: formData.endDate,
        leaveType: formData.leaveType,
        reason: formData.reason,
        selectedApproverId: manager.id,
      };

      await leaveService.submitLeave(request);

      setMessage({
        type: 'success',
        text: 'Leave request submitted successfully!',
      });

      // Reset form
      setFormData({
        leaveType: '',
        startDate: '',
        endDate: '',
        reason: '',
      });
      setAvailableDays(null);
    } catch (error: any) {
      setMessage({
        type: 'error',
        text: error.response?.data?.message || 'Failed to submit leave request',
      });
    } finally {
      setLoading(false);
    }
  };

  const daysRequested = calculateDays();

  return (
    <div className="leave-request-form">
      <h2>Request Leave</h2>

      {message && (
        <div className={`message ${message.type}`}>
          {message.text}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="approver">Approver *</label>
          {loadingManager ? (
            <div className="loading-text">Loading manager...</div>
          ) : manager ? (
            <div className="approver-info">
              <div className="approver-name">
                <strong>{manager.fullName}</strong>
              </div>
              <div className="approver-details">
                {manager.email} • {manager.department}
              </div>
            </div>
          ) : (
            <div className="error-text">
              No manager assigned. Please contact HR.
            </div>
          )}
        </div>

        <div className="form-group">
          <label htmlFor="leaveType">Leave Type *</label>
          <select
            id="leaveType"
            value={formData.leaveType}
            onChange={(e) => setFormData({ ...formData, leaveType: e.target.value })}
            required
          >
            <option value="">Select leave type</option>
            {leaveTypes.map((type) => (
              <option key={type.code} value={type.code}>
                {type.name} ({type.defaultDaysPerYear} days/year)
              </option>
            ))}
          </select>
          {availableDays !== null && (
            <span className="balance-info">
              Available: {availableDays} days
            </span>
          )}
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="startDate">Start Date *</label>
            <input
              type="date"
              id="startDate"
              value={formData.startDate}
              onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
              min={new Date().toISOString().split('T')[0]}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="endDate">End Date *</label>
            <input
              type="date"
              id="endDate"
              value={formData.endDate}
              onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
              min={formData.startDate || new Date().toISOString().split('T')[0]}
              required
            />
          </div>
        </div>

        {daysRequested > 0 && (
          <div className="days-summary">
            <strong>Total Days:</strong> {daysRequested}
            {availableDays !== null && daysRequested > availableDays && (
              <span className="insufficient-warning">
                ⚠️ Exceeds available balance
              </span>
            )}
          </div>
        )}

        <div className="form-group">
          <label htmlFor="reason">Reason *</label>
          <textarea
            id="reason"
            value={formData.reason}
            onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
            rows={4}
            placeholder="Please provide a reason for your leave request..."
            required
          />
        </div>

        <button
          type="submit"
          className="submit-btn"
          disabled={
            loading ||
            !manager ||
            loadingManager ||
            (availableDays !== null && daysRequested > availableDays)
          }
        >
          {loading ? 'Submitting...' : 'Submit Request'}
        </button>
      </form>
    </div>
  );
};

export default LeaveRequestForm;
