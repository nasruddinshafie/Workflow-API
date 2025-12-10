# Leave Management System - Frontend

A modern React TypeScript application for managing employee leave requests with workflow approvals.

## Features

- **User Selection**: Simulate login by selecting from available users
- **Dashboard**: Clean, intuitive interface showing leave balances and options
- **Leave Request Form**: Submit leave requests with real-time balance validation
- **Leave Balance Display**: View available, used, and pending leave days by type
- **Role-Based Access**: Different views for employees, managers, and HR

## Tech Stack

- **React 18** with TypeScript
- **Axios** for API communication
- **React Router** for navigation
- **Context API** for state management
- **CSS3** with modern gradients and animations

## Getting Started

### Prerequisites

- Node.js (v14 or higher)
- npm or yarn
- Backend API running on `http://localhost:5000` (configurable)

### Installation

1. Install dependencies:
   ```bash
   npm install
   ```

2. Configure API URL (optional):
   Edit `.env` file:
   ```
   REACT_APP_API_URL=http://localhost:5000/api
   ```

3. Start the development server:
   ```bash
   npm start
   ```

4. Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

## Project Structure

```
src/
├── components/         # React components
│   ├── Dashboard.tsx   # Main dashboard with sidebar
│   ├── UserSelect.tsx  # User selection screen
│   └── LeaveRequestForm.tsx  # Leave submission form
├── contexts/          # React contexts
│   └── AuthContext.tsx  # User authentication context
├── services/          # API service layer
│   ├── api.ts         # Axios configuration
│   ├── identityService.ts   # User API calls
│   ├── leaveService.ts      # Leave API calls
│   └── leaveBalanceService.ts  # Balance API calls
├── types/             # TypeScript type definitions
│   └── index.ts
├── App.tsx            # Main application component
└── index.tsx          # Application entry point
```

## Features Details

### Leave Request Submission

1. Select leave type from available options
2. Choose start and end dates
3. Enter reason for leave
4. Real-time validation:
   - Checks available balance
   - Calculates total days
   - Prevents insufficient balance submissions

### Leave Balances

View leave balances for all leave types:
- **Available**: Days you can use
- **Used**: Days already taken
- **Pending**: Days in pending approval

### User Roles

- **Employee**: Can submit and view own leave requests
- **Manager**: Can approve/reject team member requests (Coming Soon)
- **HR Manager**: Can approve/reject all requests (Coming Soon)

## API Integration

The frontend connects to the backend API at `http://localhost:5000/api` by default.

## Available Scripts

### `npm start`
Runs the app in development mode. Open [http://localhost:3000](http://localhost:3000)

### `npm test`
Launches the test runner in interactive watch mode.

### `npm run build`
Builds the app for production to the `build` folder.
