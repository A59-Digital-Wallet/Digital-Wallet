# **Virtual Wallet \- MVC .NET Project**

## **Overview**

Virtual Wallet is a web-based application built using the MVC .NET Core that allows users to manage their virtual currency. Users can perform various operations like checking balance, making transfers, viewing transaction history, requesting money from peers, checking their statistics, and many more.

## **Features**

* **User Registration & Login:** Secure user registration and login functionality with JWT and Cookie based log in. Users are registered via .Net Identity. There is a mandatory email confirmation.  
* **Profile Management:** Users can update their personal details, change password, update their profile picture. Users must confirm their email in order to log in. There is an option for a 2FA.  
* **Payment Methods:**  
  * View and manage linked debit and credit cards.  
  * Display wallet balance.  
  * Transfer money between personal wallets  
  * Exchange currency support for multiple currencies.   
  * Send money to contacts easily.  
  * Request money  
* **Transaction History:** Shows a detailed history of all transactions. Transactions can be filtered by:  
  * Type: Send or Received  
  * DateTime: Ascending or Descending  
  * Amount: Ascending or Descending  
  * Date Range: Customizable date selection, done using a binary search in order to speed up the process  
  * Wallet \-\> choose a specific one  
  * Amount  
* **Admin Panel:**  
  * Ability to block and unblock users.  
  * Admins can browse transactions of different users.  
  * Admins can change the overdraft requirements  
* **Operations:** Allows users to make 5 types of transactions on the website. This includes:  
  * **Contacts:** 4-step process: Choose Contact \> Type Transfer Details \> Confirm \> Success (could be 5 if it is a high-value transfer, then an email is sent, in order to confirm)  
  * **Deposit Money Page:** 3-step process: Deposit money \> Confirm \> Success  
  * **Withdraw Money Page:** 3-step process: Withdraw money \> Confirm \> Success (again, could be a 4 step process, if it is high value)  
  * **Transfer Between personal wallets:** 3-step process: Select Wallet \> Confirm \> Success  
  * **Request money from contact:** 3-step process: Request Money \> Request approved \> Success (could be a 4-step process, if it is a high-value transaction)  
* **Hosted services:** Our program supports hosted services that check for various different things, such as recurring transfers, savings wallet interest addition and more. Their goal is to check, given a specific time frame, whether it is time to repeat the operation.  
* **Statistics part:** There is a statistics tab where a user can view either their all time statistics regarding their wallets, or they can choose a specific time frame. Moreover, on the dashboard, there are three separate graphs \-\> weekly spending of the month for the user, spending by categories, which are optional, and a live chart that updates either daily, weekly, monthly, or yearly, showcasing the balance of each wallet. 

* **Security:**  
  * Google 2FA authentication.  
* **API Documentation with Sample Requests**  
  * Our API documentation is available via **Swagger UI**. It provides an intuitive interface for understanding our API endpoints and also showcases sample requests for each operation.  
* **Known Issues & Bugs:**  
  * **Email Limitations :** Sometimes SendGrid does not work as well as we would hope so, due to their services being slow. This is not an issue in the program but rather an issue with SendGrid. 

\#\# Technologies Used

### **This project uses several open-source technologies, including:**

* ASP.NET Core 8.0  
* ASP.NET Core 8.0 Web API  
* Swagger  
* ASP.NET MVC  
* JWT Token  
* Google 2FA  
* Cloudinary  
* SendGrid  
* Twilio  
* HTML 5  
* CSS 3  
* Microsoft Entity Framework Core  
* .Net Identity  
* Tailwind  
* JavaScript  
* Entity Framework Core InMemory  
* MSTests  
* Moq  
* Caching  
* Hosted Services  
* AJAX

# **Installation**

Follow these steps to set up and run the application:

1. **Download the App**  
   * Visit the project's repository and clone or download the app source code to your local machine.  
2. **Configure Database Connection**  
   * Locate the `appsettings.json` files in both the API and MVC projects within the solution.  
   * Modify the "DefaultConnection" string in each file to include your database server and database name details. Utilize the following format as a reference: `json "DefaultConnection": "Server=YourServerName;Database=YourDatabaseName; Integrated Security=True; MultipleActiveResultSets=True; TrustServerCertificate=True"`  
3. **Run the Application**  
   * Open the project in your preferred IDE.  
   * Add a migration and update the database. After that, run the program and the database should be set up and the program : ready to use.

Home Page - before and after login
![Home Page](https://res.cloudinary.com/dpfnd2zns/image/upload/v1725392863/1_exkjpn.png)
Login Page
![Login Page](https://res.cloudinary.com/dpfnd2zns/image/upload/v1725392860/2_nhouap.png)
Register Page
![Register Page](https://res.cloudinary.com/dpfnd2zns/image/upload/v1725392860/3_q4qki7.png)
Dashboard
![Dashboard]()
Google 2FA Page
![Google 2FA Page]()
Payment Methods Page
![Payment Methods Page]()
User Transactions Page
![User Transactions Page]()
Admin Panel Page
![Admin Panel Page]()
Contacts Page
![Contacts Page]()
Contacts History Page
![Contacts History Page]()
Statistics Page
![Statistics Page]()
User Profile Page
![User Profile Page]()
Transactions Pages
Deposit money
![Deposit Money Page]()
Withdraw 
![Withdraw Money Page]()
Transfer 
![Transfer Money Page]()
Request 
![Request Money Page]()
Transfer between personal wallets
![Transfer Between Personal Wallets Page]()
Confirm Transaction Page (2FA Enabled)
![Confirm Transaction Page (2FA Enabled)]()
Transaction Confirmation Page
![Transaction Confirmation Page]()
Note -> with a high value transfer this will include a line for the verification code
Note -> each transfer and request is sent to the wallet that is with the same currency. If this doesn’t exist, then the money is sent to the last selected wallet by the user.

API Page
![API Page](https://res.cloudinary.com/dpfnd2zns/image/upload/v1725393570/api-1_zhrbnk.png)
![API Page](https://res.cloudinary.com/dpfnd2zns/image/upload/v1725393570/api-2_tb3e1o.png)
Database Diagram
![Database Diagram]()

# **ASP.NET Framework Web Application**

This documentation explains the main architectural principles of our ASP.NET MVC 8 project, focusing on solution structure and the purpose of each project within the solution. Adhering to these principles allows us to maintain well-structured code and organized workflows.

## **Solution Structure**

The solution is composed of the following projects:

* Wallet.API  
* Wallet.Common  
* Wallet.Data  
* Wallet.DTO  
* Wallet.Services  
* Wallet.MVC

## **Wallet.API**

This project contains our API's controllers, which have the required services injected. The services for the API are configured in the **`Program.cs`** file. The database connection string is set in the **`appsettings.json`** file, while the connection itself is established in the **`Program.cs`** file.

## **Wallet.Common**

This project contains our custom exceptions and helpers. The exceptions are key for some transaction features and the helpers are there for us to format dates and information in a better manner. 

## **Wallet.Database**

The Site.Database project is responsible for defining our database tables via the **`ApplicationContext`** class. This class injects **`DbContextOptions`** for database operations. We seed the **`ApplicationContext`** class using an **`ApplicationContextSeed`** class to provide initial data. Any repositories within our architecture inject the **`ApplicationContext`** class in their constructors.

## **Wallet.DTO**

This project contains data transfer objects (DTOs) used for passing data between processes. It is organised into two main folders: **`Request`** and **`Response`**.

## **Wallet.Services**

Wallet.Services includes folders for interface definitions, service classes implementing business logic, and providers (e.g., JWT, Sendgrid, Cloudinary, Twilio, Google 2FA). The constructors for services have the necessary repositories injected. The hosted services are in a separate folder in this project. The Factory folder contains classes that map entities into DTOs and vice-versa. Our card encryption services can also be found here. 

**Site.MVC**

This is the main project for client-side interactions, containing all HTML, CSS, and JS scripts. It follows the Model-View-Controller (MVC) pattern, and the controllers are set up to have the required services injected.

## **References between Projects**

* Wallet.API: References Wallet.Services, Wallet.Database, Wallet.Common, Wallet.DTO  
* Wallet.Common: Standalone with no references to other projects  
* Wallet.Database: References Wallet.Common.  
* Wallet.DTO: References Wallet.Data, Wallet.Common.  
* Wallet.MVC: References Wallet.Services, Wallet.Database, Wallet.Common, Wallet.DTO  
* Wallet.Services: References Wallet.Data, Wallet.Common, Wallet.DTO.

# **Integration Tests Coverage Report** 

## **📊 Coverage Breakdown**

### **Services**

✅ **85.4%** test coverage.

### **API Endpoints**

✅ **94.9%** test coverage.

## **Contact Information**

For further information, please feel free to contact us:

| Contacts | Emails | Phone Number |
| ----- | ----- | ----- |
| Konstantin Peev | konstantin.i.peev@gmail.com | \+359 88 52 111 28 |
|  |  |  |
|  |  |  |
