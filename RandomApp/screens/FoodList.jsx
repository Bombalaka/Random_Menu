import { StyleSheet, Text, View, FlatList, Alert, ActivityIndicator, TouchableOpacity } from 'react-native';
import { useState, useEffect, useCallback } from 'react';
import { getFoods, deleteFood, editFood } from '../utils/api';
import { useFocusEffect } from '@react-navigation/native';
import { COLORS } from '../utils/colors';

// FoodList component receives 'route' as a parameter
// 'route' contains the data passed when navigating to this screen
const FoodList = ({ navigation }) => {
  const [isLoading, setIsLoading] = useState(true);
  const [foodlist, setFoodlist] = useState([]);

 
  //use effect to get food list from api
  useFocusEffect(
    useCallback(() => {
      getFoodList();
    }, [])
  );

//function to get food list from api
  const getFoodList = async () => {
    setIsLoading(true);
    //call api to get foods
    const result = await getFoods();
    setIsLoading(false);
    
    //handle the result
    if(result.success){
      // Backend returns { foods: [...] }, so we need to access result.data.foods
      // This keeps frontend and backend separate - if backend changes, we only change this line
      const foods = result.data?.foods || [];
      setFoodlist(foods);
  } else if(result.needsReregistration){
      // Session expired - tell user to restart
      Alert.alert(
          "Session Expired",
          "Your device registration has expired. Please restart the app to register again.",
          [{ text: "OK" }]
      );
  } else {
      // Other errors
      Alert.alert("Error", result.error || "Could not load foods");
  }
};

const handleDelete = async (foodId) => {
  // Show confirmation first
  Alert.alert(
    "Delete Food",
    "Are you sure you want to delete this food?",
    [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: async () => {
          // Call API to delete
          const result = await deleteFood(foodId);
          
          if(result.success){
            // Remove from list immediately (optimistic update)
            setFoodlist(foodlist.filter(food => food.foodId !== foodId));
            Alert.alert("Success", "Food deleted!");
          } else if(result.needsReregistration){
            Alert.alert(
              "Session Expired",
              "Please restart the app to register again.",
              [{ text: "OK" }]
            );
          } else {
            Alert.alert("Error", result.error || "Could not delete food");
          }
        }
      }
    ]
  );
};
const handleEdit = (foodName, foodId) => {
  //navigate to edit food screen and pass the food name and id
  navigation.navigate('EditFood', 
    { 
      currentFoodName: foodName, 
      foodId: foodId 
    });
};

  return (
    <View style={styles.container}>
      {isLoading ? <ActivityIndicator size="large" color="#0000ff" /> : (
        <>
          <Text style={styles.title}>Food List</Text>

          {/*if food array is empty, show a message*/}
          {foodlist.length === 0 && <Text>No food found</Text>}

          {/*if food array is not empty, show the food list*/}
          {foodlist.length > 0 && (
            <FlatList 
              data={foodlist} 
              renderItem={({ item}) => (
                <View style={styles.foodItem}>
                  {/* Backend returns FoodName (capital F), so we check both to be safe */}
                  <Text style={styles.foodName}> {typeof item === 'object' ? (item.FoodName || item.foodName) : item}</Text>
                  
                  <TouchableOpacity style={styles.deleteButton} onPress={() => handleDelete(item.foodId)}>
                    <Text>🗑️</Text>
                  </TouchableOpacity>
                  <TouchableOpacity style={styles.editButton} onPress={() => handleEdit(item.FoodName, item.foodId)}>
                    <Text>✏️</Text>
                  </TouchableOpacity>
                </View>
                    
              )} 
              keyExtractor={(item, index) => item.foodId || index.toString()} 
              style={styles.list}
              ListFooterComponent={
                <TouchableOpacity 
                  style={[styles.button, { backgroundColor: COLORS.primary, marginTop: 15 }]}
                  onPress={() => navigation.navigate('AddFood')}
                >
                  <Text style={styles.buttonText}>Add More Food</Text>
                </TouchableOpacity>
              }
            />
          )}

          {/* Show button even when list is empty */}
          {foodlist.length === 0 && (
            <TouchableOpacity 
              style={[styles.button, { backgroundColor: COLORS.primary, marginTop: 15 }]}
              onPress={() => navigation.navigate('AddFood')}
            >
              <Text style={styles.buttonText}>Add More Food</Text>
            </TouchableOpacity>
            
          )}
          
        </>
      )}
    </View>
  );
}

export default FoodList;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
    padding: 20,
    paddingTop: 50,
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 15,
    textAlign: 'center',
    color: COLORS.textDark,
  },
  list: {
    width: '100%',
  },
  foodItem: {
    fontSize: 16,
    fontWeight: 'bold',
    marginBottom: 10,
    borderWidth: 1,
    borderColor: COLORS.border,
    borderRadius: 5,
    padding: 10,
    width: '100%',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    color: COLORS.textDark,
    shadowColor: COLORS.shadow,
  },
  button: {
    padding: 10,
    borderRadius: 5,
    backgroundColor: COLORS.primary,
    marginVertical: 10,
    paddingHorizontal: 10,
    paddingVertical: 15,
    width: '100%',
    alignItems: 'center',
    shadowColor: COLORS.shadow,
  },
  buttonText: {
    color: COLORS.textMedium,
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
    
   

  },
  deleteButton: {
    
    fontSize: 22,
    fontWeight: 'bold',
    textAlign: 'center',
    paddingLeft: 10,
    
  },
  editButton: {   
    
    fontSize: 22,
    fontWeight: 'bold',
    textAlign: 'center',
    paddingLeft: 10,
  },
  foodName: {
    flex: 1,
    fontSize: 15,
    fontWeight: 'bold',
    color: COLORS.textDark,
    marginRight: 10,
  },
});
