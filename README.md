\# 🚀 Uniso Fuel Management System



A modern \*\*Fuel Management System\*\* built with ASP.NET Core MVC for managing fuel operations, including sales, inventory, fleet, users, and financial tracking.



\---



\## 📌 Overview



Uniso Fuel is a complete dashboard solution designed to manage fuel stations efficiently.  

It provides real-time insights, operational tracking, and financial reporting in one place.



\---



\## ✨ Features



\### 🔐 Authentication \& User Management

\- Login / Sign In / Switch Account

\- Role-based access (Admin, User)

\- User activity tracking

\- Profile \& security management



\### ⛽ Fuel Operations

\- Fuel Sales Management

\- Fuel Clients Management

\- Pumps Monitoring

\- Inventory Tracking (fuel stock levels)



\### 🚛 Fleet Management

\- Vehicle tracking

\- Fuel consumption monitoring

\- Trip management



\### 💰 Financial Management

\- Invoices system

\- Receipts tracking

\- Compensation (salary \& payroll)

\- Revenue insights



\### 📊 Reports \& Analytics

\- Revenue reports

\- VAT tracking

\- Collection rates

\- Export reports



\### 🔔 Notifications System

\- Real-time notifications

\- Dropdown + toast notifications

\- Notification filtering



\---





\### 🔐 Login Page

!\[Login](screenshots/login.png)



\### 🔑 Forgot Password - Step 1

!\[Forgot Password](screenshots/forgot-password.png)



\### ✅ Account Found

!\[Account Found](screenshots/account-found.png)



\### 🔁 Reset Password

!\[Reset Password](screenshots/reset-password.png)



\### 🆕 Create Account

!\[Register](screenshots/register.png)





\## 🛠️ Technologies Used



\### 💻 Backend

\- ASP.NET Core MVC (C#)

\- Entity Framework Core (ORM)

\- SQL Server (Database)



\### 🎨 Frontend

\- Razor Views (.cshtml)

\- Tailwind CSS

\- JavaScript (Vanilla JS)



\### ⚙️ Tools \& Environment

\- Visual Studio 2022

\- Git \& GitHub

\- SQL Server Management Studio (SSMS)



\---





\## 🛠️ Tech Stack



| Category   | Technology |

|------------|-----------|

| Backend    | ASP.NET Core MVC (C#) |

| ORM        | Entity Framework Core |

| Database   | SQL Server |

| Frontend   | Razor Views + Tailwind CSS |

| Scripting  | JavaScript |

| IDE        | Visual Studio 2022 |

| Tools      | Git, GitHub, SSMS |



\---



\## 🧱 Project Structure



FuelManagement/

│

├── Connected Services/

├── Dependencies/

├── Properties/

│

├── wwwroot/

│   ├── css/

│   ├── images/

│   ├── js/

│   │   ├── desktop-notifications.js

│   │   ├── mobile-notifications.js

│   │   ├── notifications.js

│   │   └── site.js

│   ├── lib/

│   ├── uploads/

│   └── favicon.ico

│

├── Controllers/

│   ├── AccountController.cs

│   ├── CompensationController.cs

│   ├── FleetMgmtController.cs

│   ├── FuelClientsController.cs

│   ├── FuelSalesController.cs

│   ├── HomeController.cs

│   ├── InventoryController.cs

│   ├── InvoicesController.cs

│   ├── NotificationController.cs

│   ├── ProfileController.cs

│   ├── PumpsController.cs

│   ├── ReceiptsController.cs

│   ├── ReportsController.cs

│   ├── TripsController.cs

│   └── UsersController.cs

│

├── Data/

│   ├── Configurations/

│   │   └── UserConfiguration.cs

│   └── ApplicationDbContext.cs

│

├── Models/

│   ├── Enums/

│   │   ├── FuelSaleStatus.cs

│   │   ├── NotificationStatus.cs

│   │   ├── NotificationType.cs

│   │   ├── PaymentMethod.cs

│   │   └── UserRole.cs

│

│   ├── ViewModels/

│   │   ├── ChangePasswordViewModel.cs

│   │   ├── NotificationFilterViewModel.cs

│   │   └── NotificationViewModel.cs

│

│   ├── (All other Models \& Feature ViewModels)

│   │   ├── Compensation.cs

│   │   ├── CompensationCreateViewModel.cs

│   │   ├── CompensationEditViewModel.cs

│   │   ├── CompensationFilterViewModel.cs

│   │   ├── CompensationListItemViewModel.cs

│   │   ├── CompensationViewModel.cs

│   │

│   │   ├── DashboardViewModel.cs

│   │   ├── DistributionItemViewModel.cs

│   │   ├── ErrorViewModel.cs

│   │

│   │   ├── Fleet.cs

│   │   ├── FleetCreateViewModel.cs

│   │   ├── FleetEditViewModel.cs

│   │   ├── FleetFilterViewModel.cs

│   │   ├── FleetListItemViewModel.cs

│   │   ├── FleetMgmtViewModel.cs

│   │

│   │   ├── ForgotPasswordViewModel.cs

│   │

│   │   ├── FuelClient.cs

│   │   ├── FuelClientFilterViewModel.cs

│   │   ├── FuelClientIndexViewModel.cs

│   │   ├── FuelClientListItemViewModel.cs

│   │   ├── FuelClientViewModel.cs

│   │

│   │   ├── FuelSale.cs

│   │   ├── FuelSaleCreateViewModel.cs

│   │   ├── FuelSaleEditViewModel.cs

│   │   ├── FuelSaleFilterViewModel.cs

│   │   ├── FuelSaleFormViewModel.cs

│   │   ├── FuelSaleListItemViewModel.cs

│   │   ├── FuelSaleViewModel.cs

│   │

│   │   ├── Inventory.cs

│   │   ├── InventoryCreateViewModel.cs

│   │   ├── InventoryEditViewModel.cs

│   │   ├── InventoryFilterViewModel.cs

│   │   ├── InventoryListItemViewModel.cs

│   │   ├── InventoryViewModel.cs

│   │

│   │   ├── Invoice.cs

│   │   ├── InvoiceCreateViewModel.cs

│   │   ├── InvoiceDetailsViewModel.cs

│   │   ├── InvoiceEditViewModel.cs

│   │   ├── InvoiceFilterViewModel.cs

│   │   ├── InvoiceListItemViewModel.cs

│   │   ├── InvoicesViewModel.cs

│   │

│   │   ├── LoginViewModel.cs

│   │   ├── MonthlySalesData.cs

│   │   ├── Notification.cs

│   │   ├── ProfileViewModel.cs

│   │

│   │   ├── Pump.cs

│   │   ├── PumpCreateViewModel.cs

│   │   ├── PumpEditViewModel.cs

│   │   ├── PumpFilterViewModel.cs

│   │   ├── PumpListItemViewModel.cs

│   │   ├── PumpsViewModel.cs

│   │

│   │   ├── Receipt.cs

│   │   ├── ReceiptCreateViewModel.cs

│   │   ├── ReceiptDetailsViewModel.cs

│   │   ├── ReceiptEditViewModel.cs

│   │   ├── ReceiptFilterViewModel.cs

│   │   ├── ReceiptListItemViewModel.cs

│   │   ├── ReceiptsViewModel.cs

│   │

│   │   ├── RecentTransactionViewModel.cs

│   │   ├── RegisterViewModel.cs

│   │

│   │   ├── ReportChartViewModel.cs

│   │   ├── ReportFilterViewModel.cs

│   │   ├── ReportListItemViewModel.cs

│   │   ├── ReportSummaryViewModel.cs

│   │   ├── ReportsViewModel.cs

│   │

│   │   ├── SwitchAccountViewModel.cs

│   │

│   │   ├── Trip.cs

│   │   ├── TripCreateViewModel.cs

│   │   ├── TripEditViewModel.cs

│   │   ├── TripFilterViewModel.cs

│   │   ├── TripListItemViewModel.cs

│   │   ├── TripsViewModel.cs

│   │

│   │   ├── User.cs

│   │   ├── UserCreateViewModel.cs

│   │   ├── UserEditViewModel.cs

│   │   ├── UserListItemViewModel.cs

│   │   ├── UserListViewModel.cs

│   │   ├── UserViewModel.cs

│   │

│   │   └── WeeklySalesData.cs

│

├── Services/

│   ├── Interfaces/

│   │   └── INotificationService.cs

│   ├── AuthService.cs

│   ├── NotificationService.cs

│   └── UserService.cs

│

├── Views/

│

│   ├── Account/

│   │   ├── ForgotPassword.cshtml

│   │   ├── Login.cshtml

│   │   ├── SignIn.cshtml

│   │   └── SwitchAccount.cshtml

│

│   ├── Compensation/

│   │   ├── Partials/

│   │   │   ├── \_CompensationFilters.cshtml

│   │   │   ├── \_CompensationForm.cshtml

│   │   │   ├── \_CompensationHeader.cshtml

│   │   │   ├── \_CompensationSummaryCards.cshtml

│   │   │   ├── \_CompensationTable.cshtml

│   │   │   └── \_DeleteCompensationModal.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── FleetMgmt/

│   │   ├── Partials/

│   │   │   ├── \_DeleteFleetModal.cshtml

│   │   │   ├── \_FleetFilters.cshtml

│   │   │   ├── \_FleetForm.cshtml

│   │   │   ├── \_FleetHeader.cshtml

│   │   │   ├── \_FleetSummaryCards.cshtml

│   │   │   └── \_FleetTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── FuelClients/

│   │   ├── Partials/

│   │   │   ├── \_DeleteFuelClientModal.cshtml

│   │   │   ├── \_FuelClientFilters.cshtml

│   │   │   ├── \_FuelClientForm.cshtml

│   │   │   ├── \_FuelClientHeader.cshtml

│   │   │   ├── \_FuelClientSummaryCards.cshtml

│   │   │   └── \_FuelClientTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── FuelSales/

│   │   ├── Partials/

│   │   │   ├── \_DeleteFuelSaleModal.cshtml

│   │   │   ├── \_FuelSaleFilters.cshtml

│   │   │   ├── \_FuelSaleForm.cshtml

│   │   │   ├── \_FuelSaleHeader.cshtml

│   │   │   ├── \_FuelSaleSummaryCards.cshtml

│   │   │   └── \_FuelSaleTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Home/

│   │   ├── Index.cshtml

│   │   ├── Privacy.cshtml

│   │   └── User.cshtml

│

│   ├── Inventory/

│   │   ├── Partials/

│   │   │   ├── \_DeleteInventoryModal.cshtml

│   │   │   ├── \_InventoryFilters.cshtml

│   │   │   ├── \_InventoryForm.cshtml

│   │   │   ├── \_InventoryHeader.cshtml

│   │   │   ├── \_InventorySummaryCards.cshtml

│   │   │   └── \_InventoryTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Invoices/

│   │   ├── Partials/

│   │   │   ├── \_DeleteInvoiceModal.cshtml

│   │   │   ├── \_InvoiceFilters.cshtml

│   │   │   ├── \_InvoiceForm.cshtml

│   │   │   ├── \_InvoiceHeader.cshtml

│   │   │   ├── \_InvoicePrintable.cshtml

│   │   │   ├── \_InvoiceSummaryCards.cshtml

│   │   │   └── \_InvoiceTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Notification/

│   │   ├── PartialDropdown.cshtml

│   │   ├── PartialFilters.cshtml

│   │   ├── PartialNotificationItem.cshtml

│   │   └── Index.cshtml

│

│   ├── Profile/

│   │   ├── Partials/

│   │   │   ├── \_ProfileHeader.cshtml

│   │   │   └── \_ProfileSidebar.cshtml

│   │   ├── Edit.cshtml

│   │   ├── Index.cshtml

│   │   └── Security.cshtml

│

│   ├── Pumps/

│   │   ├── Partials/

│   │   │   ├── \_DeletePumpModal.cshtml

│   │   │   ├── \_PumpFilters.cshtml

│   │   │   ├── \_PumpForm.cshtml

│   │   │   ├── \_PumpHeader.cshtml

│   │   │   ├── \_PumpSummaryCards.cshtml

│   │   │   └── \_PumpTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Receipts/

│   │   ├── Partials/

│   │   │   ├── \_DeleteReceiptModal.cshtml

│   │   │   ├── \_ReceiptFilters.cshtml

│   │   │   ├── \_ReceiptForm.cshtml

│   │   │   ├── \_ReceiptHeader.cshtml

│   │   │   ├── \_ReceiptPrintable.cshtml

│   │   │   ├── \_ReceiptSummaryCards.cshtml

│   │   │   └── \_ReceiptTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Reports/

│   │   ├── Partials/

│   │   │   ├── \_ReportChartSection.cshtml

│   │   │   ├── \_ReportExportModal.cshtml

│   │   │   ├── \_ReportFilters.cshtml

│   │   │   ├── \_ReportHeader.cshtml

│   │   │   ├── \_ReportSummaryCards.cshtml

│   │   │   └── \_ReportTable.cshtml

│   │   ├── Details.cshtml

│   │   └── Index.cshtml

│

│   ├── Trips/

│   │   ├── Partials/

│   │   │   ├── \_DeleteTripModal.cshtml

│   │   │   ├── \_TripFilters.cshtml

│   │   │   ├── \_TripForm.cshtml

│   │   │   ├── \_TripHeader.cshtml

│   │   │   ├── \_TripSummaryCards.cshtml

│   │   │   └── \_TripTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Delete.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   └── Index.cshtml

│

│   ├── Users/

│   │   ├── Partials/

│   │   │   ├── \_DeleteUserModal.cshtml

│   │   │   ├── \_UserActivity.cshtml

│   │   │   ├── \_UserFilters.cshtml

│   │   │   ├── \_UserForm.cshtml

│   │   │   ├── \_UserHeader.cshtml

│   │   │   ├── \_UserRolesModal.cshtml

│   │   │   ├── \_UserSidebar.cshtml

│   │   │   └── \_UsersTable.cshtml

│   │   ├── Create.cshtml

│   │   ├── Details.cshtml

│   │   ├── Edit.cshtml

│   │   ├── Index.cshtml

│   │   ├── Security.cshtml

│   │   └── UserFilters.cshtml

│

│   ├── Shared/

│   │   ├── \_AuthLayout.cshtml

│   │   ├── \_Layout.cshtml

│   │   ├── \_NotificationDropdown.cshtml

│   │   ├── \_ToastNotification.cshtml

│   │   ├── \_ValidationScriptsPartial.cshtml

│   │   └── Error.cshtml

│

│   ├── \_ViewImports.cshtml   

│   └── \_ViewStart.cshtml     

│

├── .gitignore

├── appsettings.json

├── new

└── Program.cs





\## 👤 Author



\*\*Ayub Fahim\*\*  

Yearly Class Software Development Student  



This project was independently designed and developed as part of a yearly university project, focusing on building a complete enterprise-level fuel management system using modern web technologies.



MIT License



Copyright (c) 2026 Ayub Fahim



Permission is hereby granted, free of charge, to any person obtaining a copy

of this software...





```markdown

\## 📄 License



This project is licensed under the MIT License.





The project follows a clean \*\*ASP.NET Core MVC architecture\*\*:



