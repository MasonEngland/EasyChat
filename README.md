# EasyChat

EasyChat is a lightweight, real-time, login-free web application designed for instant, disposable communication. Users can spin up a chat room in seconds, share the room ID or link, and immediately begin collaborating without the friction of account creation or app installation. Rooms are temporary and automatically self-destruct after a period of inactivity unless prolonged by the users.

**Live Demo:** https://easychat.masonengland.online

---

## Features

### Current Features
* Zero-Friction Rooms: Create a room instantly with no signup required. Share the URL or Room ID to bring others in.
* Real-Time Messaging: Powered by WebSockets for instantaneous text communication.
* File Sharing: Upload and share files directly within the chat interface.
* Real-Time Video Sharing: Seamless video distribution handled via simultaneous server-side streaming uploads and downloads.
* Local AI Chat Assistant: An integrated, fully self-hosted AI companion running in an isolated container using TinyLlama to answer prompts and assist users inside the room.
* Room Keep-Alive: A dedicated feature allowing users to extend a room's expiration timer from the standard 3-day inactivity window up to a full year.
* Disposable Infrastructure: Automated background cleanup that purges stale rooms, messages, and associated physical files after prolonged inactivity to preserve host storage and privacy.

### Planned Features
* GIF Support: Integration for rich media and animated expressions.
* Image Previews: Inline rendering for shared images.
* Saved Rooms: Local storage tracking to allow users to quickly revisit recent active rooms from their dashboard.

---

## Tech Stack & Architecture

EasyChat is built using a modern, decoupled full-stack architecture optimized for high-throughput real-time events and minimal manual maintenance:

* Frontend: React (utilizing the Context API for clean, lightweight state management and Vite as the build tool)
* Backend: ASP.NET Core web services designed using a clean separation of concerns, utilizing an MVC pattern alongside dedicated custom Service layers for business logic and background workers.
* Real-Time Layer: SignalR (managing persistent WebSocket connections)
* Database: SQLite (file-based relational database mapped via Entity Framework Core)
* Local AI: Ollama (running the TinyLlama LLM inside an isolated container)
* Infrastructure: Docker and Docker Compose (orchestrating the backend web server, database, and AI runner)
* Reverse Proxy: Nginx (handling SSL termination and traffic routing on the host)
* Deployment & CI/CD: GitHub Actions automation deploying directly to a self-hosted Linux server

---

## Room Lifecycle and Expiration Rules

To prevent server storage from bloating due to file uploads and database entries, EasyChat implements a strict background cleanup service based on activity:

* Standard Room: If a room experiences 3 days of total inactivity (no new messages or interactions), the background service automatically deletes the room, its history, and all associated files from the server.
* Keep-Alive Enabled: Clicking the "Keep Alive" button flags the room in the database, extending its expiration threshold to 1 year of inactivity.

---

## Development & Local Setup

### Prerequisites
* Docker and Docker Compose installed on your local machine.
* Node.js 20+ installed locally for compiling or serving the frontend asset layer.

### Running the Ecosystem

#### 1. Compile the Frontend Production Assets
The React frontend application does not run in a standalone Docker container. Instead, its assets must be generated locally so the backend can serve them directly:
   cd src/EasyChat.Frontend
   npm install
   npm run build

#### 2. Spin up the Backend Infrastructure
Navigate back to the project root directory and start the core server infrastructure (ASP.NET Core Web API, SQLite, and the local TinyLlama Ollama runner):
   cd ../..
   docker compose up -d --build

After the infrastructure spins up, you can access the application in your browser:
* Live Application: http://localhost:3000

Note: On the first initialization, the Ollama container will automatically pull the TinyLlama model definition. This process may take a few minutes depending on your network download speeds.

#### 3. Optional Frontend Hot-Reloading (Vite Dev Server)
If you are actively making changes to the React user interface and want instant hot-reloading without rebuilding, you can run the Vite development environment independently:
   cd src/EasyChat.Frontend
   npm run dev
* Vite Local Server: http://localhost:5173

---

## CI/CD and Deployment

EasyChat utilizes an automated Continuous Integration and Continuous Deployment (CD) pipeline via GitHub Actions.

Whenever code is pushed or merged into the master branch, the pipeline triggers, securely SSHes into the host production server, executes a git pull origin master, compiles the latest production assets, and handles the container rebuilding and up commands automatically. This guarantees automated, immutable updates, and automatically prunes dangling images to optimize host storage capacity.