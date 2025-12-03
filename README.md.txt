# 🚀 Workflow Backend API - Complete .NET 8 Integration

Complete production-ready backend API untuk integrate dengan OptimaJet Workflow Server.

---

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Docker Support](#docker-support)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## 🚀 Quick Start

### **Prerequisites:**

```bash
- .NET 8 SDK
- Workflow Server running at http://localhost:8077
- Code editor (VS Code / Rider / Visual Studio)
```

### **Step 1: Build & Run**

```bash
cd workflow-backend-api

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# API running at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### **Step 2: Test API**

```bash
# Health check
curl http://localhost:5000/api/health

# Expected:
# {"status":"Healthy","timestamp":"2025-12-02T10:30:00Z","version":"1.0.0"}
```

---

## 📁 Project Structure

```
WorkflowApi/
├── Program.cs                           # Application entry point
├── WorkflowApi.csproj                   # Project file
├── appsettings.json                     # Configuration
│
├── Controllers/
│   ├── LeaveController.cs              # Leave management endpoints
│   ├── PurchaseController.cs           # Purchase order endpoints
│   └── HealthController.cs             # Health check & admin
│
├── Services/
│   ├── IWorkflowService.cs             # Workflow service interface
│   ├── WorkflowService.cs              # Workflow Server integration
│   ├── IWorkflowSchemeRegistry.cs      # Scheme registry interface
│   └── WorkflowSchemeRegistry.cs       # Version management
│
├── Models/
│   ├── DTOs/
│   │   └── WorkflowDTOs.cs            # Data transfer objects
│   ├── Requests/
│   │   └── WorkflowRequests.cs        # Request models
│   └── Responses/
│       └── WorkflowResponses.cs       # Response models
│
├── Configuration/
│   └── WorkflowConfiguration.cs        # Configuration classes
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs      # Global error handling
│   └── LoggingMiddleware.cs            # Request logging
│
└── Extensions/
    └── ServiceCollectionExtensions.cs  # DI configuration
```

---

## ⚙️ Configuration

### **appsettings.json:**

```json
{
  "WorkflowServer": {
    "BaseUrl": "http://localhost:8077",  // ← Your Workflow Server URL
    "Timeout": 30                         // ← HTTP timeout (seconds)
  },
  
  "WorkflowConfiguration": {
    "Schemes": {
      "LeaveApproval": {
        "ActiveVersion": "LeaveApproval_v1_1_0",  // ← Current version
        "OldVersion": "LeaveApproval_v1.0.0",
        "Versions": {
          "v1.0.0": "LeaveApproval_v1.0.0",
          "v1.1.0": "LeaveApproval_v1_1_0"
        },
        "DefaultVersion": "v1.1.0"
      }
    }
  }
}
```

### **Changing Workflow Version:**

```json
// To switch to v1.2.0:
{
  "LeaveApproval": {
    "ActiveVersion": "LeaveApproval_v1_2_0",  // ← Just change this!
    "Versions": {
      "v1.0.0": "LeaveApproval_v1.0.0",
      "v1.1.0": "LeaveApproval_v1_1_0",
      "v1.2.0": "LeaveApproval_v1_2_0"        // ← Add new version
    }
  }
}

// Restart API
// New instances will use v1.2.0 automatically! ✅
```

---

## 🎯 API Endpoints

### **1. Leave Management**

#### **Submit Leave Request**

```bash
POST /api/leave/submit

Request:
{
  "employeeId": "EMP001",
  "employeeName": "Ahmad Ali",
  "startDate": "2025-12-10",
  "endDate": "2025-12-20",
  "leaveType": "Annual",
  "reason": "Family vacation"
}

Response:
{
  "success": true,
  "data": "abc-123-def-456",  // Leave ID
  "message": "Leave request submitted successfully"
}
```

#### **Get Leave Status**

```bash
GET /api/leave/{leaveId}

Response:
{
  "success": true,
  "data": {
    "leaveId": "abc-123-def-456",
    "employeeId": "EMP001",
    "employeeName": "Ahmad Ali",
    "startDate": "2025-12-10",
    "endDate": "2025-12-20",
    "totalDays": 11,
    "leaveType": "Annual",
    "reason": "Family vacation",
    "currentState": "ManagerApproval",
    "schemeVersion": "LeaveApproval_v1_1_0",
    "createdDate": "2025-12-02T10:30:00Z"
  }
}
```

#### **Manager Approve/Reject**

```bash
POST /api/leave/{leaveId}/manager-action

Request:
{
  "leaveId": "abc-123-def-456",
  "approverId": "MGR001",
  "approverName": "Manager Name",
  "comments": "Approved",
  "approved": true  // false to reject
}

Response:
{
  "success": true,
  "data": true,
  "message": "Leave approved by manager"
}
```

#### **HR Approve/Reject**

```bash
POST /api/leave/{leaveId}/hr-action

Request:
{
  "leaveId": "abc-123-def-456",
  "approverId": "HR001",
  "approverName": "HR Name",
  "comments": "Approved",
  "approved": true
}

Response:
{
  "success": true,
  "data": true,
  "message": "Leave approved by HR"
}
```

#### **Get Available Actions**

```bash
GET /api/leave/{leaveId}/actions?userId=MGR001

Response:
{
  "success": true,
  "data": [
    "ManagerApprove",
    "ManagerReject"
  ]
}
```

#### **Cancel Leave**

```bash
POST /api/leave/{leaveId}/cancel?employeeId=EMP001

Response:
{
  "success": true,
  "data": true,
  "message": "Leave request cancelled"
}
```

---

### **2. Purchase Order**

#### **Submit Purchase Order**

```bash
POST /api/purchase/submit

Request:
{
  "requestorId": "EMP001",
  "requestorName": "Ahmad Ali",
  "amount": 5000.00,
  "vendor": "Tech Supplies Inc",
  "items": [
    {
      "itemCode": "ITEM001",
      "description": "Laptop",
      "quantity": 1,
      "unitPrice": 5000.00
    }
  ],
  "description": "New laptop for development"
}

Response:
{
  "success": true,
  "data": "xyz-789-abc-123",
  "message": "Purchase order submitted successfully"
}
```

---

### **3. Admin & Health**

#### **Health Check**

```bash
GET /api/health

Response:
{
  "status": "Healthy",
  "timestamp": "2025-12-02T10:30:00Z",
  "version": "1.0.0",
  "service": "Workflow API"
}
```

#### **Get All Workflows**

```bash
GET /api/admin/workflows

Response:
{
  "success": true,
  "data": [
    "LeaveApproval",
    "PurchaseOrder",
    "DocumentApproval"
  ]
}
```

#### **Get Workflow Config**

```bash
GET /api/admin/workflows/LeaveApproval

Response:
{
  "success": true,
  "data": {
    "workflowType": "LeaveApproval",
    "activeScheme": "LeaveApproval_v1_1_0",
    "availableVersions": [
      "v1.0.0",
      "v1.1.0"
    ]
  }
}
```

---

## 🧪 Testing with cURL

### **Complete Workflow Test:**

```bash
# 1. Submit leave
LEAVE_ID=$(curl -s -X POST "http://localhost:5000/api/leave/submit" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": "EMP001",
    "employeeName": "Ahmad Ali",
    "startDate": "2025-12-10",
    "endDate": "2025-12-20",
    "leaveType": "Annual",
    "reason": "Vacation"
  }' | jq -r '.data')

echo "Leave ID: $LEAVE_ID"

# 2. Get status
curl "http://localhost:5000/api/leave/$LEAVE_ID"

# 3. Get available actions
curl "http://localhost:5000/api/leave/$LEAVE_ID/actions?userId=MGR001"

# 4. Manager approve
curl -X POST "http://localhost:5000/api/leave/$LEAVE_ID/manager-action" \
  -H "Content-Type: application/json" \
  -d '{
    "leaveId": "'$LEAVE_ID'",
    "approverId": "MGR001",
    "approverName": "Manager",
    "comments": "Approved",
    "approved": true
  }'

# 5. Check status again
curl "http://localhost:5000/api/leave/$LEAVE_ID"
```

---

## 🐳 Docker Support

### **Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkflowApi.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowApi.dll"]
```

### **docker-compose.yml:**

```yaml
version: '3.8'

services:
  workflow-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - WorkflowServer__BaseUrl=http://workflowserver:8077
    depends_on:
      - workflowserver
    networks:
      - workflow-network

  workflowserver:
    image: optimajet/workflowserver:latest
    ports:
      - "8077:8077"
    volumes:
      - ../workflow-metadata:/app/metadata
    environment:
      - DeveloperMode=true
    networks:
      - workflow-network

networks:
  workflow-network:
    driver: bridge
```

### **Run with Docker:**

```bash
# Build and run
docker-compose up -d

# Check logs
docker-compose logs -f workflow-api

# Test
curl http://localhost:5000/api/health

# Stop
docker-compose down
```

---

## 🚀 Deployment

### **Development:**

```bash
dotnet run --environment Development
```

### **Production Build:**

```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet WorkflowApi.dll
```

### **Linux Service:**

```bash
# Create service file
sudo nano /etc/systemd/system/workflow-api.service
```

```ini
[Unit]
Description=Workflow API
After=network.target

[Service]
WorkingDirectory=/opt/workflow-api
ExecStart=/usr/bin/dotnet /opt/workflow-api/WorkflowApi.dll
Restart=always
RestartSec=10
SyslogIdentifier=workflow-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=WorkflowServer__BaseUrl=http://localhost:8077

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start
sudo systemctl enable workflow-api
sudo systemctl start workflow-api
sudo systemctl status workflow-api

# View logs
sudo journalctl -u workflow-api -f
```

---

## 🔍 Features

### **✅ Implemented:**

- Complete REST API for Leave Management
- Purchase Order workflow support
- Workflow Server integration with retry & circuit breaker (Polly)
- Comprehensive logging (Serilog)
- Global error handling
- Request/Response logging
- Swagger documentation
- Health checks
- Version management (scheme registry)
- CORS enabled
- Docker support
- Production ready

### **📊 Architecture:**

```
Client (React/Angular/Mobile)
    ↓ HTTP/HTTPS
Backend API (.NET 8)  ← This project
    ↓ HTTP
Workflow Server (OptimaJet)
    ↓ SQL
Database (MSSQL/PostgreSQL)
```

---

## 🔧 Adding More Workflows

### **Step 1: Add to Configuration**

```json
// appsettings.json
{
  "WorkflowConfiguration": {
    "Schemes": {
      "LeaveApproval": { ... },
      "PurchaseOrder": { ... },
      
      "EmployeeOnboarding": {
        "ActiveVersion": "EmployeeOnboarding_v1.0.0",
        "Versions": {
          "v1.0.0": "EmployeeOnboarding_v1.0.0"
        },
        "DefaultVersion": "v1.0.0"
      }
    }
  }
}
```

### **Step 2: Create Controller**

```csharp
// Controllers/OnboardingController.cs
[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<string>>> StartOnboarding(
        [FromBody] OnboardingDto request)
    {
        var parameters = new Dictionary<string, object>
        {
            { "EmployeeId", request.EmployeeId },
            { "EmployeeName", request.EmployeeName },
            { "Department", request.Department }
        };

        var result = await _workflowService.CreateInstanceAsync(
            "EmployeeOnboarding",  // ← Workflow type from config
            request.EmployeeId,
            parameters);

        return Ok(ApiResponse<string>.SuccessResponse(
            result.ProcessId,
            "Onboarding started successfully"));
    }
}
```

---

## 🐛 Troubleshooting

### **Issue: Cannot connect to Workflow Server**

```bash
# Check Workflow Server is running
curl http://localhost:8077/health

# If not running, start it
cd /opt/workflowserver
./WorkflowServer

# Or with Docker
docker-compose up workflowserver
```

### **Issue: Scheme not found**

```json
// Check appsettings.json
{
  "WorkflowConfiguration": {
    "Schemes": {
      "LeaveApproval": {  // ← Must match workflow type in code
        "ActiveVersion": "LeaveApproval_v1_1_0"  // ← Must exist in Workflow Server
      }
    }
  }
}
```

### **Issue: 502 Bad Gateway**

```bash
# Workflow Server not responding
# Check Workflow Server logs
docker-compose logs workflowserver

# Or
tail -f /opt/workflowserver/logs/*.log
```

### **Issue: Assembly loading error**

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 📊 Monitoring

### **Logs:**

```bash
# View API logs
tail -f logs/workflow-api-*.log

# Or with systemd
sudo journalctl -u workflow-api -f
```

### **Health Check:**

```bash
# Check API health
curl http://localhost:5000/api/health

# Check Workflow Server connectivity
curl http://localhost:5000/api/admin/workflows
```

---

## ✅ Summary

### **What You Get:**

- ✅ Complete .NET 8 Web API
- ✅ Production-ready code
- ✅ Workflow Server integration
- ✅ Leave & Purchase Order workflows
- ✅ Retry & Circuit Breaker (Polly)
- ✅ Comprehensive logging (Serilog)
- ✅ Error handling middleware
- ✅ Swagger documentation
- ✅ Docker support
- ✅ Easy to extend

### **Next Steps:**

1. Configure `appsettings.json` with your Workflow Server URL
2. Run `dotnet run`
3. Open Swagger UI: http://localhost:5000/swagger
4. Test API endpoints
5. Add your own workflow types
6. Deploy to production

---

## 📞 Support

**GitHub:** [Your Repo]
**Email:** [Your Email]
**Slack:** #workflow-api

---

**Complete production-ready backend API!** 🚀

**Tested, documented, ready to deploy!** ✅