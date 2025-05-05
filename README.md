# Conestoga Virtual Game Store (CVGS)

## About

Conestoga Virtual Game Store (CVGS) is a complete web-based video game store. Users can buy games, review games, share wishlists, and register for events, as well as an administrator can manage the different features of the store. It can be deployed as a website and involves many user and administrative functionalities, which include: 

### Member Features:
- **Create an Account**: Sign up to create a customer account to get access to the Member Portal.
- **Purchase Video Games**: Buy physical or digital copies of video games.
- **My Games Library**: View and download your purchased games.
- **Wishlist**: Add games to your wishlist for future purchases.
- **Friends**: Add friends who have an account, and view and share wishlists.
- **Game Reviews**: Leave reviews and ratings for games you’ve purchased.
- **Event Sign-ups**: Register for special events.

### Administrative Features:
- **Games Management**: Add, edit, update, or delete games in the store.
- **Event Management**: Add, edit, update, or delete events.
- **Order Fulfillment**: Fulfill orders for physical games.
- **Game Moderation**: Approve or reject game reviews.
- **Reporting**: Generate reports on sales data, wishlist data, game ratings, event registrations, member orders, and more.

## Technical Architecture

This project is a web-based application built with the following technologies:

- **.NET Core Framework**
- **C#**
- **Microsoft SQL Server**

## Prerequisites

Before you run the project, you will need to set up the database.

### How to Create the Database

1. **Install SQL Server Management Studio (SSMS)**: You’ll need a DBMS like SSMS to interact with the database.
2. **Connect to SQL Server**: Use a SQL Server instance where you have permissions to create databases.
3. **Run the Database Script**:
   - Locate the file `DatabaseScriptAndData.sql` included in the repository.
   - Open this SQL file in SSMS or copy and paste the script into an SQL query window.
   - Execute the script to automatically create all required tables, relationships, views, and seed sample data into the database.

## Running the Project

### Steps to Run on Localhost

1. **Clone the Repository**:
   - Clone the repository to your local machine using Git:
     ```bash
     git clone https://github.com/emanabdeen/InsightApp.git
     ```

2. **Open the Project in Visual Studio**:
   - Launch Visual Studio and open the solution file (`.sln`).

3. **Build the Project**:
   - Ensure you have the necessary dependencies installed.
   - Build the project using Visual Studio.

4. **Run the Project**:
   - Set up the project to run on localhost by pressing **F5** or selecting **Run** from the toolbar.

5. **Login to the Admin Panel**:
   - To access the admin panel, use the following login credentials:
     - **Email**: `adamjohn@insight.com`
     - **Password**: `InsightAdminAccount123!@#`

6. **Access Member Portal**:
   - To access the Member Portal you need to create a new account and then sign in to the portal after confirming your email.

## 🔐 Configuration Notes

This project uses local secrets stored in `appsettings.Development.json`, which is ignored by Git. You'll need to create this file with your own:

- Database connection string
- Google ReCAPTCHA keys
- Email service app password

Never commit real credentials to version control.

## Additional Information

- The application includes sample data and is ready to be used immediately after setting up the database.

## Contributing

This project is publicly visible, but contributions are **restricted**.

## License

This project is licensed under an **All Rights Reserved** license, meaning no part of this project may be copied, modified, distributed, or used in any way without the express written permission of the project contributors. See the [LICENSE](LICENSE) file for details.

### Why this License?

As this is a group project, the project and all its contents are restricted to the group members only. Any use outside the group or any changes to the code require written permission from all group members. This ensures that the intellectual property and efforts of the group are protected.

---

For any issues or feature requests, please open an issue in the repository.

