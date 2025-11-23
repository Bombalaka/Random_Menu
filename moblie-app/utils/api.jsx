import axios from 'axios';
import getDeviceId from './deviceId';
import AsyncStorage from '@react-native-async-storage/async-storage';


//setup the api base url
const API_BASE_URL = 'https://mw12gj0shi.execute-api.eu-west-1.amazonaws.com/dev';

//create axios client that adds the deviceId automatically 
const apiClient = axios.create({
    baseURL: API_BASE_URL,
    //add timeout
    timeout: 10000,
});


//security checkpoint that adds the device ID badge to all requests
apiClient.interceptors.request.use(async (config) => {
  const deviceId = await getDeviceId();
  config.headers['x-device-id'] = deviceId;

  //for get requests, add the deviceId to the query parameters
  if(config.method === 'get'){
    config.params = { ...config.params, deviceId: deviceId };
  }
  return config;
});

export const addFood = async (foodName) => {
    try {
        const deviceId = await getDeviceId();
        const payload = {
            deviceId: deviceId, 
            FoodName: foodName   
        };

        console.log("📡 POST /foods payload:", payload);
        const response = await apiClient.post('/foods', payload);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const getFoods = async () => {
    //make GET request to get foods
    try {
        const response = await apiClient.get('/foods');
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

//generate menu function 
export const generateMenu = async () => {
    //make GET request to generate menu
    try {
        const response = await apiClient.get('/menu');
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const registerDevice = async (username) => {
    try {
        const deviceId = await getDeviceId();
        const payload = {
            deviceId: deviceId,
            username: username
        };
        console.log("📡 POST /register payload:", payload);
        const response = await apiClient.post('/register', payload);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const deleteFood = async (foodId) => {
    try {
        const deviceId = await getDeviceId(); //get id 
        const response = await apiClient.delete(`/foods?deviceId=${deviceId}&foodId=${foodId}`);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

// Helper to keep code clean
const handleApiError = async (error) => {
    console.log("❌ API Error:", error.message);
    if (error.response) console.log("Server Data:", error.response.data);
    
    // Check if error is "Device not registered"
    const errorMessage = error.response?.data?.message || error.response?.data?.error || error.message;
    
    // If device not registered, clear local storage
    if (errorMessage.toLowerCase().includes('not registered') || 
        errorMessage.toLowerCase().includes('unregistered')) {
        
        console.log("🔄 Device not registered in backend - clearing local storage");
        await AsyncStorage.removeItem('isRegistered');
        
        return {
            success: false,
            error: errorMessage,
            needsReregistration: true  // Flag to tell app to go back to Welcome
        };
    }
    
    return { 
        success: false, 
        error: errorMessage || 'Unknown Error'
    };
};

export const getSuggestedFoodByFavorites = async () => {
    try {
        const deviceId = await getDeviceId();
        console.log("📡 GET /suggest-food payload: deviceId:", deviceId);
        const response = await apiClient.get(`/suggest-food?deviceId=${deviceId}`);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const getSuggestedFoodByCriteria = async (criteria) => { 
    try {
        const response = await apiClient.get(`/suggest-by-criteria?&criteria=${criteria}`);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const editFood = async (foodName, foodId) => {
    try {
        const deviceId = await getDeviceId();
        const payload = {
            deviceId: deviceId,
            foodId: foodId,
            FoodName: foodName
        };
        console.log("📡 PUT /foods payload:", payload);
        const response = await apiClient.put('/foods', payload);
        return { success: true, data: response.data };
    } catch (error) {
        return handleApiError(error);
    }
};

export const getFoodImage = async (foodName) => {
    try {
        // Try full name first
        let response = await fetch(
            `https://www.themealdb.com/api/json/v1/1/search.php?s=${foodName}`
        );
        let data = await response.json();
        
        // If not found, try first word only
        if (!data.meals || data.meals.length === 0) {
            const firstWord = foodName.split(' ')[0];
            console.log(`🔍 "${foodName}" not found, trying "${firstWord}"...`);
            
            response = await fetch(
                `https://www.themealdb.com/api/json/v1/1/search.php?s=${firstWord}`
            );
            data = await response.json();
        }
        
        // Return image if found
        if (data.meals && data.meals.length > 0) {
            console.log(`✅ Found image for "${foodName}"`);
            return data.meals[0].strMealThumb;
        }
        
        // Fallback to cheese image
        console.log(`⚠️ No image found for "${foodName}", using fallback`);
        return 'https://www.themealdb.com/images/ingredients/Cheese.png';
        
    } catch (error) {
        console.log("❌ Error getting food image:", error.message);
        // Return fallback instead of null
        return 'https://www.themealdb.com/images/ingredients/Cheese.png';
    }
};