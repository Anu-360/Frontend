import React, { useEffect, useState } from 'react';
import axios from 'axios';
import Header from './Header';
import Cookies from 'js-cookie';
import {jwtDecode} from 'jwt-decode';
import CustomPagination from '../Utils/CustomPagination';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {faCircleXmark, faTimes,faPaperPlane } from '@fortawesome/free-solid-svg-icons';

const ReturnRequest = () => {
  const [returnRequests, setReturnRequests] = useState([]);
  const [loading, setLoading] = useState(false); // Add loading state
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState('');
  const [showForm, setShowForm] = useState(false);
 
  const [assetAllocations, setAssetAllocations] = useState([]);
  const [formData, setFormData] = useState({
    
    assetId:'',
    userId: '',
    assetName:'',
    returnDate: new Date().toISOString().split('T')[0],
    condition: '',
    reason: '',
    returnStatus:'',
 
  });


  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10; // Set the number of items per page

  const ConditionMapping = {
    0: 'Working',
    1: 'Damaged',
    2: 'Broken',
    
   
  };


  useEffect(() => {
    const fetchUserIdAndRequests = async () => {
      setLoading(true); 
    // Fetch the token from cookies and decode it to get the userId
    const token = Cookies.get('token');
    if (token) {
      const decode = jwtDecode(token);
      console.log('Decoded token payload:', decode);
      
      // Extract the userId from the 'nameid' field
      const userId = decode.nameid;
      
      if (userId) {
        setFormData((prevFormData) => ({
          ...prevFormData,
          userId: userId // Set the userId in formData
        }));

        try {
          // Fetch return requests
          const returnResponse = await axios.get('https://localhost:7287/api/ReturnRequests', {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });
          console.log('Return Requests fetched:', returnResponse.data);
          setReturnRequests(returnResponse.data);

          // Fetch asset allocations
          const assetResponse = await axios.get(`https://localhost:7287/api/AssetAllocations/user/${userId}`, {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });
          console.log('Asset Allocations fetched:', assetResponse.data);
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

  fetchUserIdAndRequests(); // Call the function to fetch data
}, []); // Empty dependency array to run only on mount


const handleAssetNameChange = (e) => {
  const selectedAssetName = e.target.value.trim();
  const selectedAsset = assetAllocations.find(asset => asset.assetName.toLowerCase() === selectedAssetName.toLowerCase());
  
  console.log("Selected Asset Name:", selectedAssetName);
  console.log("Available Assets:", assetAllocations);
  console.log("Selected Asset:", selectedAsset);

  if (selectedAsset) {
      console.log("Asset ID Found:", selectedAsset.assetId);
      console.log("Category Id found", selectedAsset.categoryId);
      setFormData({
          ...formData,
          assetName: selectedAssetName,
          assetId: selectedAsset.assetId ,
          categoryId: selectedAsset.categoryId
      });
  } else {
      console.warn("No matching asset found for:", selectedAssetName);
      setFormData({
          ...formData,
          assetName: selectedAssetName,
          assetId: '' 
      });
  }
};


  const handleFormSubmit = async (e) => {
    e.preventDefault();
    console.log("Submitting Return Request:", formData);
    try {
      const response = await axios.post('https://localhost:7287/api/ReturnRequests', {
          ...formData // Ensure that formData is correctly populated
      });
      console.log("Return Request Submitted:", response.data);
  } catch (error) {
      console.error("Error submitting return request:", error);
     

  }
   
    // Reset form after submission
    setFormData({
        
      assetId:'',
      userId: '',
      assetName:'',
      returnDate: new Date().toISOString().split('T')[0],
      condition: '',
      reason: '',
      returnStatus:'',
    });
    setShowForm(false);
    setSuccessMessage('Return request sent successfully!');
    setTimeout(() => setSuccessMessage(''), 3000);
  };

  // Calculate the index of the last item and the first item of the current page
  const indexOfLastRequest = currentPage * itemsPerPage;
  const indexOfFirstRequest = indexOfLastRequest - itemsPerPage;

  // Slice the filteredRequests to get the requests for the current page
  const currentRequests = returnRequests.slice(indexOfFirstRequest, indexOfLastRequest);

  return (
    <div className="min-h-screen bg-white">
    <Header />
    <div className="flex items-center justify-center bg-white">
      <div className="max-w-7xl w-full mx-auto py-20 px-6 bg-white shadow-lg rounded-lg">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-center w-full">RETURNS</h1>
        </div>

        {/* Display loading or error messages */}
        {loading ? (
          <p>Loading return requests...</p>
        ) : error ? (
          <p className="text-red-500">{error}</p>
        ) : (
          <>

        {/* Table for Return Requests */}
        <table className="w-full text-left table-auto border-collapse">
          <thead>
            <tr className="text-lg font-medium text-gray-700 border-b">
              <th className="px-4 py-2">Return Id</th>
              <th className="px-4 py-2">Asset Id</th>
              <th className="px-4 py-2">Return Date</th>
              <th className="px-4 py-2">Reason</th>
              <th className="px-4 py-2">Condition</th>
              <th className="px-4 py-2">Request Status</th>
            </tr>
          </thead>
          <tbody>
            {returnRequests.map((request) => (
              <tr key={returnRequests.returnId} className="text-lg border-b">
                <td className="px-4 py-2">{request.returnId}</td>
                <td className="px-4 py-2">{request.assetId}</td>
                <td className="px-4 py-2">{new Date(request.returnDate).toLocaleDateString()}</td>
                <td className="px-4 py-2">{request.reason}</td>
                <td className="px-4 py-2">{request.condition}</td>
                <td className="px-4 py-2">
                {request.returnStatus === 0 ? (
    <span className="text-blue-600 font-semibold"><FontAwesomeIcon icon={faPaperPlane} /> Sent</span>
  ) : request.returnStatus === 1 ? (
    <span className="text-yellow-500 font-semibold"><FontAwesomeIcon icon={faThumbsUp}/> Approved</span>
  ) : request.returnStatus === 2 ?(
    <span className="text-green-500 font-semibold"><FontAwesomeIcon icon={faCircleCheck}/> Returned</span>
  ):(
    <span className="text-red-600 font-semibold"><FontAwesomeIcon icon={faCircleXmark}/> Rejected</span>
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
                totalItems={returnRequests.length}
                itemsPerPage={itemsPerPage}
                onPageChange={setCurrentPage}
              />
       
        
        {/* Terms and Conditions */}
        <div className="p-10 mt-8  flex justify-between items-start">
                <div className="mt-4 bg-gray-100 p-4 rounded-lg w-1/2 shadow-lg">
          <h2 className="text-lg font-bold">Terms and Conditions</h2>
          <p className="mt-2 text-sm">
            Employees must return assets in their original or acceptable condition, as outlined in the asset management policy. 
            Any damages or missing components will incur charges and may affect future asset allocations.
          </p>
        </div>
       

        {/* Button at the Bottom */}
        <div className="mt-8 flex flex-col items-center w-1/2 ">
          <button className="bg-red-500 text-white py-2 px-4 rounded hover:bg-red-600"  onClick={()=>setShowForm(true)}>
            Raise a Return Request
          </button>
          {showForm && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <form onSubmit={handleFormSubmit} className="bg-white p-5 rounded shadow-lg text-center w-1/4 h-45">
            <div className="flex justify-between items-center mb-4">
              <div className="flex-grow text-center">
                <h3 className="text-lg text-indigo-950 font-bold">Return Request</h3>
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
          {assetAllocations.map((allocation) => (
            <option key={allocation.userId} value={allocation.assetName}  className="bg-indigo-950 text-slate-200 ">
              {allocation.assetName} {/* Assuming 'assetName' exists on each allocation */}
            </option>
          ))}
        </select>
                          </div>

{/* Asset Condition Dropdown */}
<div className="relative">
  <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">
    Asset Condition
  </label>
  <select
    value={formData.condition}
    onChange={(e) => setFormData({ ...formData, condition: e.target.value })}
    className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
    required
  >
    <option value="" disabled>Select Asset Condition</option>
    {Object.keys(ConditionMapping).map((key) => (
      <option key={key} value={ConditionMapping[key]} className="bg-indigo-950 text-slate-200">
        {ConditionMapping[key]}
      </option>
    ))}
  </select>
</div>


                 {/* Request Date Field */}
                 <div className="relative">
                <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Request Date</label>
                <input
                  type="date"
                  value={formData.returnDate}
                  readOnly
                  className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
                />
              </div>

                          {/* Service Description Field */}
                          <div className="relative">
                            <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Return Reason</label>
                            <textarea
                              value={formData.reason}
                              onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                              placeholder="Describe the reason"
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

export default ReturnRequest;

