import { StyleSheet, Text, View, FlatList, Alert, ActivityIndicator, TouchableOpacity, TextInput } from 'react-native';
import { useState, useEffect } from 'react';
import { getFoods, deleteFood, editFood } from '../utils/api';
import { COLORS } from '../utils/colors';

// FoodList component receives 'route' as a parameter
// 'route' contains the data passed when navigating to this screen
const EditFood = ({ navigation, route }) => {
  const [isLoading, setIsLoading] = useState(false); //we did not load data from api yet and we load from route params
  const { foodId, currentFoodName } = route.params; //get from foodlist screen
  const [ newFoodName, setNewFoodName] = useState(currentFoodName); //state for new food name

const handleEdit = async () => {
    //validate the food name not empty
    if(newFoodName.trim() === ""){
        Alert.alert("Please enter a food name");
        return;
    }
    //chake did name acutlly change or not
    
    if(currentFoodName.trim() === newFoodName.trim()){
        Alert.alert("No changes made");
        return;
    }
    setIsLoading(true);
    //call api to edit food
    const result = await editFood(newFoodName, foodId);
    setIsLoading(false);
    //handle 3 cases: success, needs reregistration, error
    if(result.success){
        //go back to food list screen
        navigation.goBack();
        Alert.alert("Success", "Food edited!");
    } else if(result.needsReregistration){
        Alert.alert("Session Expired", "Your device registration has expired. Please restart the app to register again.");
    } else {
        Alert.alert("Error", result.error || "Could not edit food");
    }
};

  return (
    <View style={styles.container}>
    <Text style={styles.title}>Edit Food</Text>
    
    <Text>Current Food: {currentFoodName}</Text>
    
    <TextInput 
      style={styles.input} 
      placeholder="Enter New Name" 
      value={newFoodName} 
      onChangeText={setNewFoodName} 
    />
    
    <TouchableOpacity 
      style={[styles.button, {backgroundColor: COLORS.primary}]} 
      onPress={handleEdit} //Just call it, no parameters needed
      disabled={isLoading}
    >
      <Text style={styles.buttonText}>
        {isLoading ? 'Saving...' : 'Save Changes'}
      </Text>
    </TouchableOpacity>
    
    <TouchableOpacity 
      style={[styles.button, {backgroundColor: COLORS.primary}]} 
      onPress={() => navigation.goBack()}
      disabled={isLoading}
    >
      <Text style={styles.buttonText}>Cancel</Text>
    </TouchableOpacity>
  </View>
  );
}

export default EditFood;

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
  },
  buttonText: {
    color: COLORS.textMedium,
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',

  },
  deleteButton: {
    color: COLORS.textMedium,
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
  },
  input: {
    width: '80%',
    height: 40,
    borderColor: COLORS.border,
    borderWidth: 2,
    marginBottom: 10,
    paddingHorizontal: 10,
    borderRadius: 5,
    color: COLORS.textDark,
    backgroundColor: COLORS.cardBackground,
    shadowColor: COLORS.shadow,
  },
  inputText: {
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
    color: COLORS.textDark,
  },
});
