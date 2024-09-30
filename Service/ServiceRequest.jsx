import React, { useEffect, useState } from 'react';
import axios from 'axios';
import Header from './Header';
import Cookies from 'js-cookie';
import {jwtDecode} from 'jwt-decode';
import CustomPagination from '../Utils/CustomPagination';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCircleExclamation, faCircleRight,faThumbsUp,faTimes } from '@fortawesome/free-solid-svg-icons';

const ServiceRequest = () => {
  const [serviceRequests, setServiceRequests] = useState([]);
  const [loading, setLoading] = useState(false); // Add loading state
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState('');
  const [showForm, setShowForm] = useState(false);
 
  const [assetAllocations, setAssetAllocations] = useState([]);
  const [formData, setFormData] = useState({
    userId: '',
    assetName:'',
    assetId: '',
    serviceRequestDate: new Date().toISOString().split('T')[0],
    issue_Type: '',
    serviceDescription: '',
    serviceReqStatus: '',
  });

  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10; // Set the number of items per page




  
  const issueTypeMapping = {
    0: 'Malfunction',
    1: 'Repair',
    2: 'Installation',
    
   
  };

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true); // Start loading
      const token = Cookies.get('token');
      if (token) {
        const decode = jwtDecode(token);
        const userId = decode.nameid;
  
        if (userId) {
          setFormData((prevFormData) => ({
            ...prevFormData,
            userId: userId,
          }));
  
          try {
            // Fetch service requests
            const serviceResponse = await axios.get('https://localhost:7287/api/ServiceRequests', {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            });
            setServiceRequests(serviceResponse.data);
  
            // Fetch asset allocations
            const assetResponse = await axios.get(`https://localhost:7287/api/AssetAllocations/user/${userId}`, {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            });
            const assetAllocationsData = Array.isArray(assetResponse.data.$values) ? assetResponse.data.$values : assetResponse.data;
            setAssetAllocations(assetAllocationsData);
          } catch (error) {
            setError('Error fetching data');
          } finally {
            setLoading(false); // End loading
          }
        } else {
          setError('User ID not found in token payload');
        }
      } else {
        setError('Token not found');
      }
    };
  
    fetchData();
  }, []);



  const handleAssetNameChange = (e) => {
    const selectedAssetName = e.target.value.trim();
    const selectedAsset = assetAllocations.find(asset => asset.assetName.toLowerCase() === selectedAssetName.toLowerCase());
    
    console.log("Selected Asset Name:", selectedAssetName);
    console.log("Available Assets:", assetAllocations);
    console.log("Selected Asset:", selectedAsset);

    if (selectedAsset) {
        console.log("Asset ID Found:", selectedAsset.assetId);
        setFormData({
            ...formData,
            assetName: selectedAssetName,
            assetId: selectedAsset.assetId // Auto-fetch the assetId
        });
    } else {
        console.warn("No matching asset found for:", selectedAssetName);
        setFormData({
            ...formData,
            assetName: selectedAssetName,
            assetId: '' // Reset assetId if no match found
        });
    }
};



const handleFormSubmit = async (e) => {
  e.preventDefault();
  console.log("Submitting Service Request:", formData); // Log formData before submission
  try {
      const response = await axios.post('https://localhost:7287/api/ServiceRequests', {
          ...formData // Ensure that formData is correctly populated
      });
      console.log("Service Request Submitted:", response.data);
  } catch (error) {
      console.error("Error submitting service request:", error);
     

  }
  setFormData({
    assetId: '',
    userId: '',
    assetName: '',
    serviceRequestDate: new Date().toISOString().split('T')[0],
    issue_Type: '',
    serviceDescription: ''
});
setShowForm(false);
setSuccessMessage('Service request sent successfully!');
setTimeout(() => setSuccessMessage(''), 3000);
};

   
  

  // Calculate the index of the last item and the first item of the current page
  const indexOfLastRequest = currentPage * itemsPerPage;
  const indexOfFirstRequest = indexOfLastRequest - itemsPerPage;

  // Slice the filteredRequests to get the requests for the current page
  const currentRequests = serviceRequests.slice(indexOfFirstRequest, indexOfLastRequest);

 

  return (
    <div className="min-h-screen bg-white">
      <Header />
      <div className="flex items-center justify-center bg-white">
        <div className="max-w-7xl w-full mx-auto py-20 px-6 bg-white shadow-lg rounded-lg">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-center w-full">SERVICES</h1>
          </div>

          {/* Display loading or error messages */}
          {loading ? (
            <p>Loading service requests...</p>
          ) : error ? (
            <p className="text-red-500">{error}</p>
          ) : (
            <>
              {/* Table for Service Requests */}
              <table className="w-full text-left table-auto border-collapse">
                <thead>
                  <tr className="text-lg font-medium text-gray-700 border-b">
                    <th className="px-4 py-2">Service Id</th>
                    <th className="px-4 py-2">Asset Id</th>
                    <th className="px-4 py-2">Asset Name</th>
                    <th className="px-4 py-2">Request Date</th>
                    <th className="px-4 py-2">Issue Type</th>
                    <th className="px-4 py-2">Description</th>
                    <th className="px-4 py-2">Service Status</th>
                  </tr>
                </thead>
                <tbody>
                  {serviceRequests.map((request) => (
                    <tr key={request.serviceId} className="text-lg border-b">
                      <td className="px-4 py-2">{request.serviceId}</td>
                      <td className="px-4 py-2">{request.assetId}</td>
                      <td className="px-4 py-2">{request.asset.assetName}</td>
                      <td className="px-4 py-2">{new Date(request.serviceRequestDate).toLocaleDateString()}</td>
                      <td>{issueTypeMapping[request.issue_Type] || 'Unknown'}</td> 
                      <td className="px-4 py-2">{request.serviceDescription}</td>
                      <td className="px-4 py-2">
  {request.serviceReqStatus === 0 ? (
    <span className="text-red-600 font-semibold"><FontAwesomeIcon icon={faCircleExclamation} /> Under Review</span>
  ) : request.serviceReqStatus === 1 ? (
    <span className="text-yellow-500 font-semibold"><FontAwesomeIcon icon={faThumbsUp}/> Approved</span>
  ) : (
    <span className="text-green-500 font-semibold"><FontAwesomeIcon icon={faCircleCheck}/> Completed</span>
  )}
</td>
                    </tr>
                  ))}
                </tbody>
              </table>

        {/* Pagination */}
        <div className="my-12"/>
        <CustomPagination
                currentPage={currentPage}
                totalItems={serviceRequests.length}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
              />
             

           
             {/* Service Guidelines and Button Layout */}
             <div className="p-10 mt-8  flex justify-between items-start">
                <div className="mt-4 bg-gray-100 p-4 rounded-lg w-1/2 shadow-lg">
                  <h2 className="text-lg font-bold">SERVICE GUIDELINES</h2>
                  <ul className="mt-2 text-lg">
  <li className="flex justify-between items-center">
    <span>What issues can be reported</span>
    <FontAwesomeIcon icon={faCircleRight} className="text-red-500 text-sm mx-2" />
    <span>Malfunction, Repair, Installation</span>
  </li>
  <li className="flex justify-between items-center">
    <span>Expected turnaround time</span>
    <FontAwesomeIcon icon={faCircleRight} className="text-red-500 text-sm mx-2" />
    <span>Two Weeks from Request Date</span>
  </li>
</ul>

                </div>
               
                
                {/* Button at the Right Side */}
                <div className="mt-8 flex flex-col items-center w-1/2 ">
                  <button className="bg-red-500 text-white py-2 px-6 rounded hover:bg-red-600" 
                  onClick={()=>setShowForm(true)}>
                    Raise a Service Request
                  </button>
                  {/* Overlapping Form for Asset Request */}
      {showForm && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <form onSubmit={handleFormSubmit} className="bg-white p-5 rounded shadow-lg text-center w-1/4 h-45">
            <div className="flex justify-between items-center mb-4">
              <div className="flex-grow text-center">
                <h3 className="text-lg text-indigo-950 font-bold">Service Request</h3>
              </div>
              {/* Close Button beside the heading */}
              <FontAwesomeIcon
                icon={faTimes}
                className="text-red-500 cursor-pointer ml-2" // Add margin for spacing
                onClick={() => setShowForm(false)} // Close the form
              />
            </div>
            
            <div className="flex flex-col space-y-4 mt-4">

          

              {/* User ID Field */}
              <div className="relative">
                <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">User ID</label>
                <input
                  type="text"
                  value={formData.userId}
                  placeholder="User ID"
                  readOnly
                  className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
                />
              </div>

              
      {/* Asset Name Dropdown */}
      <div className="relative">
                            <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Asset Name</label>
                            <select
          value={formData.assetName}
          onChange={handleAssetNameChange}
          className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
          required
        >
          <option value="" disabled>Select an Asset</option>
          {assetAllocations.map((asset) => (
            <option key={asset.assetId} value={asset.assetName}  className="bg-indigo-950 text-slate-200 ">
              {asset.assetName} {/* Assuming 'assetName' exists on each allocation */}
            </option>
          ))}
        </select>
                          </div>
                               
   


                          {/* Issue Type Dropdown */}
                          <div className="relative">
                            <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Issue Type</label>
                            <select
                              value={formData.issue_Type}
                              onChange={(e) => setFormData({ ...formData, issue_Type: e.target.value })}
                              className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
                              required
                            >
                              <option value="" disabled >Select Issue Type</option>
                              {Object.keys(issueTypeMapping).map((key) => (
                                <option key={key} value={key} className="bg-indigo-950 text-slate-200 ">{issueTypeMapping[key]}</option>
                              ))}
                            </select>
                          </div>

                             {/* Request Date Field */}
              <div className="relative">
                <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Request Date</label>
                <input
                  type="date"
                  value={formData.serviceRequestDate}
                  readOnly
                  className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
                />
              </div>

                          {/* Service Description Field */}
                          <div className="relative">
                            <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Description</label>
                            <textarea
                              value={formData.serviceDescription || ''}
                              onChange={(e) => setFormData({ ...formData, serviceDescription: e.target.value })}
                              placeholder="Describe the issue"
                              className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
                              rows="4"
                            />
                          </div>
           
   
          

         

              <button
                type="submit"
                className="bg-indigo-950 text-white px-4 py-2 rounded mt-4"
              >
                Submit Request
              </button>
            </div>
          </form>
        </div>
      )}
                </div>
              </div>
            </>
          )}
            {successMessage && (
          <div className="fixed top-5 right-5 bg-green-500 text-white p-3 rounded shadow-lg">
            {successMessage}
          </div>
        )}
        </div>
      </div>
    </div>
    
  );
};

export default ServiceRequest;

