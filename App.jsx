import './App.css'
import{BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import HexaHubLandingPage from './Components/LandingPage/HomePage';
import SignInPage from './Components/SignInPage/Signin';
import Dashboard from './Components/AdminView/DashBoard/DashBoard';
import { ThemeProvider } from './Components/ThemeContext';
import Employee from './Components/AdminView/EmployeePage/Employee';
import UpdateUser from './Components/AdminView/EmployeePage/UpdateUser'
import UserDetails from './Components/AdminView/EmployeePage/EmployeeInfo';
import EmpDashboard from './Components/Emp_Dashboard/EmpDashboard';
import Asset from './Components/Assets/Assets';
import Notifications from './Components/Notifications/Notify';
import ServiceRequest from './Components/Service/ServiceRequest';
import ReturnRequest from './Components/ReturnRequest/ReturnReq';
import Profile from './Components/Profile/Profile';
import ExtraPage from './Components/ExtraPage/settings';
import Audit from './Components/Audit/Audit';


function App() {
  return (
    <ThemeProvider >
    <Router>
      <Routes>
        <Route path = "/admin/Dashboard" element={<Dashboard />}/>
        <Route path="/" element={<HexaHubLandingPage />} />
        <Route path="/signin" element={<SignInPage />} />
        <Route path='/employee' element={<Employee/>}/>
        <Route path="/user/update/:id" element={<UpdateUser />} />
        <Route path="/user/:id" element = {<UserDetails/>}/>
        <Route path ="/Asset" element={<Asset/>}/>
        <Route path ="/Notification" element={<Notifications/>}/>
        <Route path="/Profile" element={<Profile/>}/>
        <Route path="/EmpDashboard" element={<EmpDashboard />} />
        <Route path="/ServiceRequest" element={<ServiceRequest/>}/>
        <Route path="/ReturnRequest" element={< ReturnRequest/>}/>
        <Route path="/Settings" element={<ExtraPage/>}/>
        <Route path="/Audit" element={<Audit/>}/>

      </Routes>
    </Router>
    </ThemeProvider>
  )
}

export default App;
