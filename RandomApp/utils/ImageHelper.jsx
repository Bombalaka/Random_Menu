

 
const PEXELS_API_KEY = process.env.EXPO_PUBLIC_PEXELS_API_KEY;  


//get food image from pixels.com
export const getPixelsImage = async (foodName) => {
    try {
        const response = await fetch(
          `https://api.pexels.com/v1/search?query=${foodName}&per_page=1`,
          {
            headers: {
              Authorization: PEXELS_API_KEY
            }
          }
        );
        const data = await response.json();
        
        if (data.photos && data.photos.length > 0) {
          return {
            imageUrl: data.photos[0].src.medium,
            photographer: data.photos[0].photographer
          };
        }
        return null;
    } catch (error) {
        console.log('Pexels error:', error);
        return null;
    }
}
//get food image from themealdb.com
export const searchTheMealDB = async (foodName) => {
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
        return null;
        
    } catch (error) {
        console.log("❌ Error getting food image:", error.message);
        // Return fallback instead of null
        return null;
    }
};

//get foods image id mealdb dones not has image so method call from pixels.com
export const getFoodsImage = async (foodName) => {
    // Try TheMealDB first
  const mealDbImage = await searchTheMealDB(foodName);
  if (mealDbImage) {
    return {
      type: 'mealdb',
      url: mealDbImage,
      credit: null
    };
  }
  // Try Pexels second
  const pexelsImage = await getPixelsImage(foodName);
  if (pexelsImage) {
    return {
      type: 'pexels',
      url: pexelsImage.imageUrl,
      credit: `Photo by ${pexelsImage.photographer} on Pexels`
    };
  }
  
}