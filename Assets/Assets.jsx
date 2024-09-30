import React, { useState, useEffect ,useRef} from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSearch, faFilter, faEllipsisV, faTimes } from '@fortawesome/free-solid-svg-icons'; // Importing faTimes for the close icon
import Header from './Header';
import axios from 'axios';
import CustomPagination from '../Utils/CustomPagination';
import Cookies from 'js-cookie';
import {jwtDecode} from 'jwt-decode';

const Assets = () => {
  const [assetsData, setAssetsData] = useState([]);
  const [maxValue, setMaxValue] = useState(100000);
  const [filteredAssets, setFilteredAssets] = useState([]);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [showPrompt, setShowPrompt] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [isCategoryDropdownOpen, setCategoryDropdownOpen] = useState(false);
  const [isLocationDropdownOpen, setLocationDropdownOpen] = useState(false);
  const [isAssetStatusDropdownOpen, setAssetStatusDropdownOpen] = useState(false);
  const [locations, setLocations] = useState([]);
  const [categories, setCategory] = useState([]);
  const [assetStatuses, setAssetStatuses] = useState([]);
  const [minValue, setMinValue] = useState(0); 
  const [currentMaxValue, setCurrentMaxValue] = useState(maxValue);
 

  const [selectedLocation, setSelectedLocation] = useState('');
  const [selectedAssetStatus, setSelectedAssetStatus] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');


  const [formData, setFormData] = useState({
    assetId: '',
    userId: '',
    assetName: '',
    assetReqDate: new Date().toISOString().split('T')[0],
    requestReason: '',
    assetType: '',
    request_Status:'',
    categoryId:''
  });

  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 4;



  useEffect(() => {
    fetchAssets();
}, []);

const fetchAssets = async () => {
  try {
    const response = await axios.get('https://localhost:7287/api/Assets/assetall');
    console.log('Fetched assets:', response.data);

    const data = Array.isArray(response.data) ? response.data : [];
    const assetsWithImages = await Promise.all(data.map(async (asset) => {
      console.log('Fetching image for asset ID:', asset.assetId); // Log asset ID
      try {
        const imageResponse = await axios.get(`https://localhost:7287/api/Assets/get-image/${asset.assetId}`, {
          responseType: 'blob', // Set response type to blob to handle binary image data
        });

        // Create an Object URL from the Blob
        const imageUrl = URL.createObjectURL(imageResponse.data);
        console.log('Image URL for asset ID:', asset.assetId, imageUrl);
        
        return {
          ...asset,
          imageUrl: imageUrl, // Assign the Object URL
        };
      } catch (imageError) {
        console.error(`Error fetching image for asset ${asset.assetId}:`, imageError);
        return {
          ...asset,
          imageUrl: null, // Default to null if image fetch fails
        };
      }
    }));

    setFilteredAssets(assetsWithImages);
    setAssetsData(data);
    setFilteredAssets(data);

    // Extract unique locations and statuses
    const uniqueLocations = [...new Set(data.map(asset => asset.location))];
    setLocations(uniqueLocations);
    
    const uniqueStatus = [...new Set(data.map(asset => asset.assetStatusName))]; 
    setAssetStatuses(uniqueStatus);

    const uniqueCategories = [...new Set(data.map(asset => asset.categoryName))]; 
    setCategory(uniqueCategories);


    if (data.length > 0) {
      const values = data.map(asset => asset.value);
      setMinValue(Math.min(...values));
      setMaxValue(Math.max(...values));
      setCurrentMaxValue(Math.max(...values)); // Set current max value initially to max
    } else {
      setMinValue(0);
      setMaxValue(100000); // Reset to some default value if no assets
      setCurrentMaxValue(100000);
    }
  } catch (error) {
    console.error('Error fetching assets data:', error.response ? error.response.data : error.message);
    setAssetsData([]);
    setFilteredAssets([]);
  }
};





 const openPrompt = (asset) => {
    if (asset.assetStatusName === 'Allocated') { 
      setErrorMessage(`Asset "${asset.assetName}" is already allocated. Please select available assets.`);
      return; 
    }
    setSelectedAsset(asset);
    setShowPrompt(true);
    setErrorMessage(''); 
   
  }
  useEffect(() => {
    let timer;
    if (errorMessage) {
      timer = setTimeout(() => {
        setErrorMessage('');
      }, 3000);
    }
    return () => clearTimeout(timer); // Cleanup timeout if the component unmounts
  }, [errorMessage]);


  
  useEffect(() => {
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
      } else {
        console.error('User ID not found in token payload');
      }
    }
  }, []);

  const confirmPrompt = () => {
    setFormData((prevFormData) => ({
      ...prevFormData,
      assetId: selectedAsset.assetId,
      userId:'',
      assetName: selectedAsset.assetName,
      assetReqDate: new Date().toISOString().split('T')[0], // current date
      requestReason: '',
      assetType: selectedAsset.categoryName,
      categoryId: selectedAsset.categoryId,
      request_Status: 'Pending' // Example status
    }));
    console.log("SubCategory", selectedAsset.subCategoryId);
    setShowPrompt(false);
    setShowForm(true);
  };


  const handleFormSubmit = async (e) => {
    e.preventDefault();
    console.log("Form Data to Submit:", formData); // Log the form data

    try {
        const response = await axios.post('https://localhost:7287/api/AssetRequests', {
            ...formData
        });
        console.log("Asset Request Submitted:", response.data);
    } catch (error) {
        console.error("Error submitting asset request:", error);
    }


  // Reset form after submission
  setFormData({
    assetId: '',
    userId: '',  // Reset userId if necessary
    assetName: '',
    assetReqDate: new Date().toISOString().split('T')[0],
    requestReason: '',
    assetType: '',
    request_Status: '',
    categoryId: ''
  });
  setShowForm(false);
  setSuccessMessage('Asset request sent successfully!');
  setTimeout(() => setSuccessMessage(''), 3000);
};

  useEffect(() => {
    if (Array.isArray(assetsData)) {  // Ensure assetsData is an array before filtering
      const filtered = assetsData.filter((asset) => asset.value <= currentMaxValue);
      setFilteredAssets(filtered);
    }
  }, [currentMaxValue, assetsData]);

  const handleSearch = async (e) => {
    const value = e.target.value;
    setSearchTerm(value);

    if (value) {
        try {
            // Use encodeURIComponent to safely encode the asset name
            const response = await axios.get(`https://localhost:7287/api/Assets/ByAssetName/${encodeURIComponent(value)}`);
            console.log('API Response:', response.data);
           // Set filteredAssets to the response data
           const assets = Array.isArray(response.data) ? response.data : [];

        

           setFilteredAssets(assets);
        } catch (error) {
            console.error('Error fetching assets:', error);
        }
    } else {
        setFilteredAssets([]); // Clear results if search term is empty
    }
};


  const dropdownRef = useRef(null);
  const locationDropdownRef=useRef(null);
  const categorydropdownRef = useRef(null);
  const assetstatusdropdownRef = useRef(null);


  const handleLocationSelect = (location) => {
    setSelectedLocation(location);
    setLocationDropdownOpen(false); // Close dropdown after selection
    // Filter assets based on the selected location
    const filtered = assetsData.filter(asset => asset.location === location);
    setFilteredAssets(filtered);
  };


  const handleAssetStatusSelect = (status) => {
    setSelectedAssetStatus(status);
    setAssetStatusDropdownOpen(false); // Close dropdown after selection
  
    // Optionally, filter assets based on the selected asset status
    const filtered = assetsData.filter(asset => asset.asset_Status === status);
    setFilteredAssets(filtered);
  };
  
  
  const handleCategorySelect = (category) => {
    setSelectedCategory(category);
    setCategoryDropdownOpen(false); // Close dropdown after selection
  
    // Optionally, filter assets based on the selected asset status
    const filtered = assetsData.filter(asset => asset.categoryName === category);
    setFilteredAssets(filtered);
  };

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Toggle dropdown visibility
  const toggleDropdown = () => {
    setDropdownOpen((prev) => !prev);
  };
  
  // Handle clicks outside the dropdown
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (categorydropdownRef.current && !categorydropdownRef.current.contains(event.target)) {
        setCategoryDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Toggle dropdown visibility
  const toggleCategoryDropdown = () => {
    setCategoryDropdownOpen((prev) => !prev);
  };

  

useEffect(() => {
  const handleClickOutside = (event) => {
    if (locationDropdownRef.current && !locationDropdownRef.current.contains(event.target)) {
      setLocationDropdownOpen(false);
    }
  };
  document.addEventListener('mousedown', handleClickOutside);
  return () => {
    document.removeEventListener('mousedown', handleClickOutside);
  };
}, []);

  // Toggle dropdown visibility
  const toggleLocationDropdown = () => {
    setLocationDropdownOpen((prev) => !prev);
  };
   

  
  // Handle clicks outside the dropdown
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (assetstatusdropdownRef.current && !assetstatusdropdownRef.current.contains(event.target)) {
        setAssetStatusDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Toggle dropdown visibility
  const toggleAssetStatusDropdown = () => {
    setAssetStatusDropdownOpen((prev) => !prev);
  };

  

  

  return (
    <div className="min-h-screen bg-white">
      <Header />
      <div className="p-6 bg-white">
        <header className="mb-4">
          <div className="flex justify-center">
            <h1 className="text-2xl font-bold text-indigo-950">Assets</h1>
            </div>

          {/* Search and Filter Section */}
          <div className="p-4 flex justify-center mt-4">
            <div className="flex items-center space-x-4">
              <div className="relative">
                <FontAwesomeIcon
                  icon={faSearch}
                  className="absolute left-2 top-2 text-red-400"
                />
                <input
                  type="text"
                  placeholder="Search for Assets..."
                  value={searchTerm}
                  onChange={handleSearch}
                  className="pl-10 pr-4 py-2 bg-white border rounded-md focus:outline-none focus:ring-2 focus:ring-black text-indigo-950"
                />
              </div>
             

              <div className="flex items-center pl-5">
                <FontAwesomeIcon
                  icon={faFilter}
                  className="text-xl text-red-400 hover:text-red-500 cursor-pointer"
                  onClick={toggleDropdown}
  />
</div>

<div className="relative">
  {isDropdownOpen && (
    <div ref={dropdownRef} className="absolute right-0 mt-5 w-48 bg-indigo-950 border rounded-lg shadow-lg z-10">
      <ul className="py-2">
        {/* Category Dropdown */}
        <li className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer relative" onClick={toggleCategoryDropdown}>
          Category
          {isCategoryDropdownOpen && (
            <div className="absolute left-full top-0 mt-2 w-40 bg-indigo-950 border rounded-lg shadow-lg z-10">
              <ul className="py-2">
              {categories.map(category => (
                <li 
                key={category}
                className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer"
                onClick={() => handleCategorySelect(category)}
                  >{category}</li>
              ))}
               
              </ul>
            </div>
          )}
        </li>

        {/* Location Dropdown */}
        <li className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer relative" onClick={toggleLocationDropdown}>
          Location
          {isLocationDropdownOpen && (
            <div className="absolute left-full top-0 mt-2 w-40 bg-indigo-950 border rounded-lg shadow-lg z-10">
              <ul className="py-2">
              {locations.map(location => (
                <li 
                key={location}
                className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer"
                onClick={() => handleLocationSelect(location)}
                  >{location}</li>
              ))}
                
              </ul>
            </div>
          )}
        </li>

        {/* Asset Status Dropdown */}
        <li className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer relative" onClick={toggleAssetStatusDropdown}>
          Asset Status
          {isAssetStatusDropdownOpen && (
            <div className="absolute left-full top-0 mt-2 w-40 bg-indigo-950 border rounded-lg shadow-lg z-10">
              <ul className="py-2">
              {assetStatuses.map(assetStatus=> (
                <li 
                key={assetStatus}
                className="px-4 py-2 text-slate-200 hover:bg-red-500 cursor-pointer"
                onClick={() => handleAssetStatusSelect(assetStatus)}
                  >{assetStatus}</li>
              ))}
              </ul>
            </div>
          )}
        </li>
      </ul>
    </div>
  )}
</div>


<div className="flex flex-col items-start pl-12">
    

      {/* Range Slider */}
      <div className="flex items-center space-x-2">
        <input
          type="range"
          min={minValue} 
          max={maxValue}
          value={currentMaxValue} 
          onChange={(e) => setCurrentMaxValue(parseInt(e.target.value))}
          className="slider"
        />
         <span className="text-red-400 font-bold">₹{currentMaxValue}</span>
      </div>

     
    </div>
            </div>
          </div>
        </header>

        {/* Assets Display */}
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {filteredAssets.length > 0 ? (filteredAssets.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage).map((asset) => (
            <div
              key={asset.assetId}
              className="bg-stone-100 shadow-lg shadow-zinc-400 rounded-lg overflow-hidden flex flex-col justify-between cursor-pointer"
            >   
             <div className="flex justify-center items-center">
             {asset.imageUrl ? (
          <img src={asset.imageUrl} alt={`Asset ${asset.assetId}`} style={{ maxWidth: '100%', maxHeight: '200px' }} />
        ) : (
          <img src ="Images/default.webp" className="object-cover w-48 h-48" />
        )}
      </div>
              <div className="p-4 text-left relative">
                <h2 className="text-xl font-bold text-indigo-950">{asset.assetName}</h2>
                <p className="text-sm text-gray-600">{asset.assetDescription}</p>
                <p className="text-gray-800">Serial No: {asset.serialNumber}</p>
                <p className="text-red-400 font-bold">₹{asset.value}</p>
                <p className="text-gray-800">Location: {asset.location}</p>
                <p className="text-gary-800 font-semibold" >{asset.categoryName}</p>
                <p className="text-gray-800">Model: {asset.model}</p>
                <span className={`text-sm font-semibold ${asset.assetStatusName === 'OpenToRequest' ? 'text-green-500' : 'text-red-500'}`}>
                  {asset.assetStatusName}
                </span>
                <div className="absolute bottom-2 right-6">
                  <FontAwesomeIcon
                    icon={faEllipsisV}
                    className="text-indigo-950 cursor-pointer"
                    onClick={() => openPrompt(asset)} // Opens the prompt with the asset details
                  />
                </div>
              </div>
            </div>
          ))) : (
            <p className="text-center text-gray-500">No assets found.</p>
          )}
        </div>

        {/* Confirmation Prompt */}
        {showPrompt && (
          <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-white p-5 rounded shadow-lg text-center">
              <p className="mt-4">Are you sure you want to request the asset <strong>{selectedAsset?.assetName}</strong>?</p>
              <div className="flex justify-center mt-4">
                <button
                  className="bg-indigo-950 text-white px-4 py-2 rounded mr-2"
                  onClick={confirmPrompt}
                >
                  Confirm
                </button>
                <button
                  className="bg-red-500 text-white px-4 py-2 rounded"
                  onClick={() => setShowPrompt(false)}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}

{/* Overlapping Form for Asset Request */}
{showForm && (
  <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
    <form onSubmit={handleFormSubmit} className="bg-white p-5 rounded shadow-lg text-center w-1/4 h-45">
      
      <div className="flex justify-between items-center mb-4">
        <div className="flex-grow text-center">
          <h3 className="text-lg text-indigo-950 font-bold">Asset Request</h3>
        </div>
        {/* Close Button beside the heading */}
        <FontAwesomeIcon
          icon={faTimes}
          className="text-red-500 cursor-pointer ml-2" // Add margin for spacing
          onClick={() => setShowForm(false)} // Close the form
        />
      </div>
      
      <div className="flex flex-col space-y-4 mt-4">

        {/* Asset ID Field */}
        <div className="relative">
          <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Asset ID</label>
          <input
            type="text"
            value={formData.assetId}
            placeholder="Asset ID"
            readOnly
            className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
          />
        </div>

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

        {/* Asset Name Field */}
        <div className="relative">
          <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Asset Name</label>
          <input
            type="text"
            value={formData.assetName}
            placeholder="Asset Name"
            readOnly
            className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
          />
        </div>

        {/* Request Date Field */}
        <div className="relative">
          <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Request Date</label>
          <input
            type="date"
            value={formData.assetReqDate}
            readOnly
            className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
          />
        </div>
        
        {/*Asset Type Field */}
        <div className="relative">
          <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Asset Type</label>
          <input
            type="text"
            value={formData.assetType}
            readOnly
            className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
          />
        </div>

        {/* Request Reason Field */}
        <div className="relative">
          <label className="absolute -top-3 left-3 px-1 bg-white text-sm font-semibold text-slate-500">Request Reason</label>
          <textarea
            className="p-3 border-2 bg-white border-slate-200 rounded w-full text-indigo-950 focus:outline-none"
            placeholder="Enter reason for asset request"
            value={formData.requestReason}
            onChange={(e) => setFormData({ ...formData, requestReason: e.target.value })}
            required
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


        {/* Pagination */}
        <CustomPagination
          currentPage={currentPage}
          totalItems={filteredAssets.length}
          itemsPerPage={itemsPerPage}
          onPageChange={setCurrentPage}
        />

         {successMessage && (
          <div className="fixed top-5 right-5 bg-green-500 text-white p-3 rounded shadow-lg">
            {successMessage}
          </div>
        )}
        {errorMessage && (
          <div className="fixed top-5 right-5 bg-red-500 text-white p-3 rounded shadow-lg">
            {errorMessage}
          </div>
        )}
      </div>
    </div>
  );
};

export default Assets;












