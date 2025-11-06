# Dublin Shop Finder

A web application to search for shops around Dublin using location-based queries. Built with **Angular frontend** and **ASP.NET Core 9 backend**, using **Google Places API** to fetch shop information.

---

## Table of Contents
- **Features**  
- **Tech Stack**  
- **Google Cloud Setup**  
- **Installation**  
- **Running the Application**  
- **API Endpoints**  
- **CORS Configuration**  
- **Notes**  
- **License**

---


## Features

- **Search for shops by name or category**  
- **Detect user location** using browser geolocation  
- **Fetch shop data from Google Places API**  
- **Adjustable search radius** (default: 5000 meters)  

---

## Tech Stack

- **Frontend:** Angular 17  
- **Backend:** ASP.NET Core 9  
- **External API:** Google Places API  
- **Other:** HTML, CSS, TypeScript

---

## Google Cloud Setup

1. Go to **Google Cloud Console**: [https://console.cloud.google.com/](https://console.cloud.google.com/)  
2. Create a **new project** or select an existing project.  
3. Enable the **Places API**:  
   - Navigate to **APIs & Services → Library**  
   - Search for **Places API** → Enable  
4. Create an **API key**:  
   - Go to **APIs & Services → Credentials → Create Credentials → API Key**  
   - Copy the key  
5. (Optional) Restrict the API key:  
   - **HTTP referrers:** 
   - **IP addresses:**  
6. Add the API key to your backend via **appsettings.json** or environment variables:    
    ```json
    {
      "GooglePlacesApiKey": "YOUR_API_KEY_HERE"
    }

---

## Installation

### Backend (ASP.NET Core 9 API)

1. **Clone the repository:**
  ```bash
  git clone <your-repo-url>
  cd ShopFinderBackend
```
2. **Configure your Google Places API key** in appsettings.json
3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
4. **Trust the development certificate for HTTPS:**
   ```bash
   dotnet dev-certs https --trust
   ```
5. **Run the API**
   ```bash
   dotnet run
   ```

### Frontend (Angular)

1. **Navigate to the frontend folder:**
   ```bash
   cd ShopFinderFrontend
   ```
2. **Install dependencies:**
   ```bash
   npm install
   ```
3. **Run the Angular app:**
   ```bash
   ng serve
   ```

### Running the Application

1. **Start the ASP.NET Core API** first.
2. **Start the Angular frontend.**
3. **Open a browser** and navigate to:
4. **Enter a query** in the search box (e.g., clothes, cafe).
5. **Allow location access** for accurate search results.

### API Endpoints

**Query Parameters:**
| Parameter | Type   | Description                               |
|-----------|--------|-------------------------------------------|
| query     | string | Name or category of the shop (required)   |
| lat       | double | Latitude of user location (required)      |
| lng       | double | Longitude of user location (required)     |
| radius    | int    | Search radius in meters (default: 5000)   |


   
