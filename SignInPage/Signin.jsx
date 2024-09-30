/* eslint-disable no-unused-vars */
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import axios from 'axios';
import { jwtDecode } from 'jwt-decode';
import Cookies from 'js-cookie';

const SignInPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [userName, setUserName] = useState('');
  const [gender, setGender] = useState('');
  const [dept, setDept] = useState('');
  const [designation, setDesignation] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [address, setAddress] = useState('');
  const [branch, setBranch] = useState('');
  const [error, setError] = useState('');
  const [userId, setUserId] = useState(null);
  const navigate = useNavigate();

  const handleLogoClick = (e) => {
    e.preventDefault();
    navigate('/');
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    const loginData = {
      UserMail: email,
      Password: password,
    };

    try {
      const response = await axios.post('https://localhost:7287/api/Auth', loginData);

      const { token } = response.data;
      Cookies.set('token', token);
      //Decoding JWT token for fetching User Role and to navigate to appropirate dashboard
      const decode = jwtDecode(token);
      console.log(decode);
   

      setUserId(decode.userId);
      const userRole = decode.role;
      if (userRole == 'Admin') {
        navigate('/admin/Dashboard');
      }
      else {
        navigate('/EmpDashBoard');
      }
      toast.success('Login Successful!', {
        position: 'top-right',
        autoClose: 2000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });
      console.log("Success");
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Invalid credentials';
      console.error('Error during login:', err);

      toast.error(errorMessage, {
        position: 'top-right',
        autoClose: 2000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });

      setError(errorMessage);
    
    }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    const RegisterData = {
      UserName: userName,
      UserMail: email,
      Gender: gender,
      Dept: dept,
      Designation: designation,
      PhoneNumber: phoneNumber,
      Address: address,
      Password: password,
      Branch: branch,
    };
    try {
      await axios.post('https://localhost:7287/api/Users', RegisterData);
      console.log(RegisterData);
      setTimeout(() => {
        navigate('/');
      }, 3000);
      toast.success('Registration Successful!, Please Wait a moment.', {
        position: 'top-right',
        autoClose: 2000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });
      console.log("Success");
    }
    catch (err) {
      const errorMessage = err.response?.data?.message || 'Registration failed';
      toast.error(errorMessage, {
        position: 'top-right',
        autoClose: 2000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });
    }
  }
  const [isSignUp, setIsSignUp] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col justify-center px-4 sm:px-6">
      <div className="absolute top-0 left-0 p-4 sm:p-6 md:p-8 hidden lg:contents mt-6 sm:mt-12">
        <div className="flex items-center mt-5 bg-transparent">
          <img
            alt="HexaHub Logo"
            src="../Images/logo.png"
            className="h-10 w-auto cursor-pointer bg-transparent"
            onClick={handleLogoClick}
          />
          <p
            className="text-xl font-semibold text-gray-900 ml-2 cursor-pointer"
            onClick={handleLogoClick}
          >
            HexaHub
          </p>
        </div>

      </div>

      <div className="mb-8 sm:mb-12 lg:hidden mt-6 sm:mt-8">
        <a href="/" onClick={handleLogoClick} className="flex items-center justify-center">
          <img alt="HexaHub Logo" src="../Images/logo.png" className="h-10 w-auto" />
          <p className="text-xl font-semibold text-gray-900 ml-2">HexaHub</p>
        </a>
      </div>

      <button
        onClick={() => navigate('/')}
        className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
      >
        ✖
      </button>

      <div className="w-full max-w-md mx-auto">
        <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
          {isSignUp ? 'Create an HexaHub Account' : 'Sign in to HexaHub'}
        </h2>

        <div className="mt-8 bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <form className="space-y-4" onSubmit={isSignUp ? handleRegister : handleLogin}>
            {isSignUp && (
              <input className=" text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Username" value={userName} onChange={(e) => setUserName(e.target.value)} />
            )}
            <input
              className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md"
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
            <div className="relative">
              <input
                className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md"
                type={showPassword ? 'text' : 'password'}
                placeholder="Password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
              <button
                type="button"
                className="bg-transparent absolute inset-y-0 right-0 pr-3 flex items-center focus:outline-none"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? '👁️' : '👁️‍🗨️'}
              </button>
            </div>
            {isSignUp && (
              <>
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Gender" value={gender} onChange={(e) => setGender(e.target.value)} />
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Department" value={dept} onChange={(e) => setDept(e.target.value)} />
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Designation" value={designation} onChange={(e) => setDesignation(e.target.value)} />
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Phone Number" value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} />
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Address" value={address} onChange={(e) => setAddress(e.target.value)} />
                <input className="text-black bg-white w-full px-3 py-2 border border-gray-300 rounded-md" placeholder="Branch" value={branch} onChange={(e) => setBranch(e.target.value)} />
              </>
            )}
            <div>
              <button
                type="submit"
                className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-[#F87060] hover:bg-[#f85842] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#F87060]"
              >
                {isSignUp ? 'Create' : 'Sign In'}
              </button>
            </div>
          </form>

          <ToastContainer />

          <div className="mt-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300"></div>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">
                  {isSignUp ? 'Already have an account?' : "Don't have an account?"}
                </span>
              </div>
            </div>

            <div className="mt-6">
              <button
                onClick={() => setIsSignUp(!isSignUp)}
                className="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
              >
                {isSignUp ? 'Sign In' : 'Create Account'}
              </button>
            </div>
          </div>

          <div className="mt-6 flex items-center justify-between text-sm">
            <a href="#" className="font-medium text-indigo-600 hover:text-indigo-500">Privacy</a>
            <a href="#" className="font-medium text-indigo-600 hover:text-indigo-500">Terms</a>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignInPage;
