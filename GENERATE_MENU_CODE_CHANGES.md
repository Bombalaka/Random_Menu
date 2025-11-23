# Generate Menu Feature - Complete Code Changes

This document contains all the code changes made to implement the Generate Menu functionality using React Context for shared state management.

---

## 📁 File Structure

```
moblie-app/
├── context/
│   └── FoodContext.js          (NEW FILE)
├── App.js                        (MODIFIED)
└── screens/
    ├── AddFood.jsx               (MODIFIED)
    ├── GenerateMenu.jsx         (MODIFIED)
    └── FoodList.jsx              (MODIFIED)
```

---

## 1. NEW FILE: `context/FoodContext.js`

**Purpose:** Creates a shared context to store the food list that all screens can access.

**Location:** `moblie-app/context/FoodContext.js`

```javascript
import React, { createContext, useState, useContext } from 'react';

// Create a Context - this is like a "storage box" that all screens can access
// Context is React's way to share data between components without passing props
const FoodContext = createContext();

// This is a Provider component - it wraps your app and provides the foodList to all screens
export const FoodProvider = ({ children }) => {
  // This state will be shared across all screens
  const [foodList, setFoodList] = useState([]);

  // This function allows any screen to add a food to the list
  const addFood = (foodName) => {
    setFoodList(prevList => [...prevList, foodName]);
  };

  // This function allows any screen to clear the food list
  const clearFoodList = () => {
    setFoodList([]);
  };

  // The value object contains everything we want to share
  const value = {
    foodList,        // The current list of foods
    addFood,         // Function to add a food
    clearFoodList,   // Function to clear the list
    setFoodList      // Function to set the list directly (if needed)
  };

  return (
    <FoodContext.Provider value={value}>
      {children}
    </FoodContext.Provider>
  );
};

// This is a custom hook - it makes it easy to use the context in any component
// Instead of writing useContext(FoodContext) every time, we use useFood()
export const useFood = () => {
  const context = useContext(FoodContext);
  if (!context) {
    throw new Error('useFood must be used within a FoodProvider');
  }
  return context;
};
```

---

## 2. MODIFIED: `App.js`

**Changes:**
- Added import for `FoodProvider`
- Wrapped the entire app with `<FoodProvider>` to make foodList available to all screens

**Location:** `moblie-app/App.js`

```javascript
import { StatusBar } from 'expo-status-bar';
import { StyleSheet, Text, View, TouchableOpacity, Image } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { FoodProvider } from './context/FoodContext';  // ← ADDED
import AddFood from './screens/AddFood';
import GenerateMenu from './screens/GenerateMenu';
import FoodList from './screens/FoodList';

const Stack = createNativeStackNavigator();

// HomeScreen needs to receive 'navigation' as a parameter
// This is how React Navigation passes the navigation object to your screen
function HomeScreen({ navigation }) {
  return (
    <View style={styles.container}>
      <Image source={require('./assets/logo-cat.png')} style={styles.logo} />
      <Text style={styles.title}>Welcom to the Random Menu App!</Text>
      
      <View style={styles.buttonContainer}>
        <TouchableOpacity 
          style={styles.button} 
          onPress={() => navigation.navigate('AddFood')}
        >
          <Text style={styles.buttonText}>Add Food</Text>
        </TouchableOpacity>

        <TouchableOpacity 
          style={styles.button} 
          onPress={() => navigation.navigate('GenerateMenu')}
        >
          <Text style={styles.buttonText}>Generate Menu</Text>
        </TouchableOpacity>
      </View>
    </View>
  )
}

export default function App() {
  return (
    // FoodProvider wraps everything so all screens can access the foodList
    <FoodProvider>  {/* ← ADDED: Wraps entire app */}
      <NavigationContainer>
        <Stack.Navigator>
          <Stack.Screen 
            name="Home" 
            component={HomeScreen}
            options={{ title: 'Random Menu App' }}
          />
          <Stack.Screen 
            name="AddFood" 
            component={AddFood}
            options={{ title: 'Add Food' }}
          />
          <Stack.Screen 
            name="GenerateMenu" 
            component={GenerateMenu}
            options={{ title: 'Generate Menu' }}
          />
          <Stack.Screen 
            name="FoodList" 
            component={FoodList}
            options={{ title: 'Food List' }}
          />
        </Stack.Navigator>
      </NavigationContainer>
    </FoodProvider>  {/* ← ADDED: Closing tag */}
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
    
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 20,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: 5,
  },
  button: {
    padding: 10,
    borderRadius: 5,
    backgroundColor: '#333',
    marginVertical: 20,
    paddingHorizontal: 25,
    paddingVertical: 15,
    width: '45%',
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
  },
  logo: {
    width: 100,
    height: 100,
    borderRadius: 30,
    marginBottom: 20,
  },
});
```

---

## 3. MODIFIED: `screens/AddFood.jsx`

**Changes:**
- Removed local `foodList` state
- Added `useFood()` hook to access shared context
- Updated `handleAddFood` to use `addFood()` from context
- Updated `handleClearFoodList` to use `clearFoodList()` from context
- Updated `handleShowFoodList` to not pass foodList (FoodList uses context now)

**Location:** `moblie-app/screens/AddFood.jsx`

```javascript
import { StyleSheet, Text, View, TouchableOpacity, Alert, TextInput } from 'react-native';
import { useState } from 'react';
import { useFood } from '../context/FoodContext';  // ← ADDED

// AddFood component needs to receive 'navigation' as a parameter
// This allows it to navigate to other screens
const AddFood = ({ navigation }) => {
    // Get foodList and functions from the shared context
    // This way, the foodList is shared with GenerateMenu screen
    const { foodList, addFood, clearFoodList } = useFood();  // ← CHANGED: Uses context
    
    // Local state only for the input field
    const [foodName, setFoodName] = useState("");

    const handleAddFood = () => {
        if(foodName == ""){
            Alert.alert("Please enter a food name");
            return;
        }
        // Use the addFood function from context to add food to shared list
        addFood(foodName);  // ← CHANGED: Uses context function
        setFoodName(""); // Clear the input after adding
    }

    const handleClearFoodList = () => {
        // Use the clearFoodList function from context
        clearFoodList();  // ← CHANGED: Uses context function
        setFoodName("");
    }

    const handleShowFoodList = () => {
        // No need to pass foodList anymore - FoodList uses context now!
        navigation.navigate('FoodList');  // ← CHANGED: No params needed
    }

    return (
        <View style={styles.container}>
            <Text style={styles.title}>Add Food Name</Text>
            {/*the feedback loop for the food name*/}
            <TextInput style={styles.input} placeholder="Enter Food Name" value={foodName} onChangeText={setFoodName} />
            {/*the button for add food and clear food list*/}
            
                <TouchableOpacity style={styles.button} onPress={handleAddFood}>
                    <Text style={styles.buttonText}>Add Food</Text>
                </TouchableOpacity>

                
        
                <TouchableOpacity style={styles.button} onPress={handleShowFoodList}>
                    <Text style={styles.buttonText}>show food list</Text>
                </TouchableOpacity>
           
                <TouchableOpacity style={styles.button} onPress={handleClearFoodList}>
                    <Text style={styles.buttonText}>Clear Food List</Text>
                </TouchableOpacity>
           
        </View>
    );
};

export default AddFood;

const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#f0f0f0',
    },
    title: {
        fontSize: 20,
        fontWeight: 'bold',
        color: '#333',
    },
    button: {
        padding: 10,
        borderRadius: 5,
        backgroundColor: '#333',
        marginVertical: 10,
    },
    buttonText: {
        color: '#fff',
        fontSize: 16,
        fontWeight: 'bold',
        textAlign: 'center',
    },
    input: {
        width: '80%',
        height: 40,
        borderColor: 'gray',
        borderWidth: 1,
        marginBottom: 10,
        paddingHorizontal: 10,
    },
    buttonContainer: {
        flexDirection: 'row',
        justifyContent: 'center',
        width: '100%',

    },
    foodName: {
        fontSize: 16,
        fontWeight: 'bold',
        color: '#333',
        textAlign: 'center',
        marginVertical: 10,

    },
});
```

---

## 4. MODIFIED: `screens/GenerateMenu.jsx`

**Changes:**
- Added `useFood()` hook to access shared foodList
- Removed fake random check
- Updated `generateMenu()` to actually pick a random food from the foodList
- Added better UI with food count display
- Fixed duplicate code and improved error handling

**Location:** `moblie-app/screens/GenerateMenu.jsx`

```javascript
import { StyleSheet, Text, View, TouchableOpacity, Alert } from 'react-native';
import { useState } from 'react';
import { useFood } from '../context/FoodContext';  // ← ADDED

const GenerateMenu = () => {
  // Get foodList from the shared context
  // Now GenerateMenu can see all the foods added in AddFood screen!
  const { foodList } = useFood();  // ← ADDED: Gets foodList from context
  
  //set state for menu == null
  const [menu, setMenu] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  //function to generate menu and check if user has food  
  const generateMenu = () => {
    // Check if user has any food in the list
    if(foodList.length === 0){  // ← CHANGED: Real check instead of fake random
      Alert.alert("No food found", "Please add some foods first!");
      return;
    }
    
    setIsLoading(true);
    
    // Simulate loading time (like calling an API)
    setTimeout(() => {
      // Pick a random food from the foodList
      // Math.random() gives a number between 0 and 1
      // Multiply by foodList.length to get a number between 0 and length
      // Math.floor() rounds down to get a valid array index
      const randomIndex = Math.floor(Math.random() * foodList.length);  // ← CHANGED: Real random selection
      const selectedFood = foodList[randomIndex];
      
      // Create menu object with the randomly selected food
      const generatedMenu = {
        foodName: selectedFood,  // ← CHANGED: Uses actual food from list
        foodImage: 'https://via.placeholder.com/150', // You can add real images later
      };
      
      setMenu(generatedMenu);
      setIsLoading(false);
    }, 2000);
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Generate Random Menu</Text>
      
      {/* Show how many foods are available */}
      {foodList.length === 0 ? (
        <Text style={styles.subtitle}>No foods available. Add some foods first!</Text>
      ) : (
        <Text style={styles.subtitle}>
          {foodList.length} food{foodList.length > 1 ? 's' : ''} available
        </Text>
      )}
      
      {/*if menu is existing, show it*/}
      {menu != null && (
        <View style={styles.menuContainer}>
          <Text style={styles.menuTitle}>Your Random Menu:</Text>
          <Text style={styles.menuFood}>{menu.foodName}</Text>
        </View>
      )}
      
      {/*if not generated yet show button generate menu*/}
      {menu == null && !isLoading && (
        <TouchableOpacity style={styles.button} onPress={generateMenu}>
          <Text style={styles.buttonText}>Generate Menu</Text>
        </TouchableOpacity>
      )}
      
      {/*generate again button*/}
      {menu != null && !isLoading && (
        <TouchableOpacity style={styles.button} onPress={generateMenu}>
          <Text style={styles.buttonText}>Generate Again</Text>
        </TouchableOpacity>
      )}
      
      {/*if menu is loading show loading*/}
      {isLoading && <Text style={styles.loading}>Loading...</Text>}
    </View>
  );
};

export default GenerateMenu;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 10,
    color: '#333',
  },
  subtitle: {
    fontSize: 14,
    color: '#666',
    marginBottom: 30,
    textAlign: 'center',
  },
  button: {
    padding: 15,
    borderRadius: 5,
    backgroundColor: '#333',
    marginVertical: 10,
    minWidth: 200,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
  },
  menuContainer: {
    marginVertical: 30,
    padding: 20,
    backgroundColor: '#f0f0f0',
    borderRadius: 10,
    alignItems: 'center',
  },
  menuTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 10,
    color: '#333',
  },
  menuFood: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#333',
  },
  loading: {
    fontSize: 18,
    color: '#666',
    marginTop: 20,
  },
});
```

---

## 5. MODIFIED: `screens/FoodList.jsx`

**Changes:**
- Removed `route` parameter (no longer needed)
- Added `useFood()` hook to access shared foodList
- Removed `useEffect` and local state (uses context directly)
- Improved styling

**Location:** `moblie-app/screens/FoodList.jsx`

```javascript
import { StyleSheet, Text, View, FlatList } from 'react-native';
import { useFood } from '../context/FoodContext';  // ← ADDED

// FoodList component now uses the shared context
// This way it always shows the current food list from AddFood screen
const FoodList = () => {
  // Get foodList directly from the shared context
  // No need to pass data through navigation anymore!
  const { foodList } = useFood();  // ← CHANGED: Uses context instead of route params

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Food List</Text>

      {/*if food array is empty, show a message*/}
      {foodList.length === 0 && (
        <Text style={styles.emptyText}>No food found. Add some foods first!</Text>
      )}

      {/*if food array is not empty, show the food list*/}
      {foodList.length > 0 && (
        <FlatList 
          data={foodList} 
          renderItem={({ item, index }) => (
            <View style={styles.foodItem}>
              <Text style={styles.foodName}>• {item}</Text>
            </View>
          )} 
          keyExtractor={(item, index) => index.toString()} 
        />
      )}
    </View>
  );
}

export default FoodList;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 20,
    color: '#333',
  },
  emptyText: {
    fontSize: 16,
    color: '#666',
    textAlign: 'center',
    marginTop: 20,
  },
  foodItem: {
    padding: 10,
    marginVertical: 5,
  },
  foodName: {
    fontSize: 18,
    color: '#333',
  },
});
```

---

## 📝 Summary of Changes

### Key Concepts Used:

1. **React Context API**
   - Created a shared storage (`FoodContext`) for the food list
   - All screens can access and update the same food list
   - No need to pass data through navigation parameters

2. **Custom Hook (`useFood`)**
   - Makes it easy to access the context in any component
   - Provides `foodList`, `addFood()`, and `clearFoodList()`

3. **Random Selection Algorithm**
   ```javascript
   const randomIndex = Math.floor(Math.random() * foodList.length);
   const selectedFood = foodList[randomIndex];
   ```
   - `Math.random()` generates a number between 0 and 1
   - Multiply by array length to get a valid index
   - `Math.floor()` rounds down to get an integer index

### How It Works:

1. **AddFood Screen**: Uses `addFood()` from context to add foods to shared list
2. **GenerateMenu Screen**: Uses `foodList` from context to pick a random food
3. **FoodList Screen**: Uses `foodList` from context to display all foods
4. **All screens** share the same `foodList` state through the context

---

## ✅ Testing Steps

1. Add foods in the "Add Food" screen
2. Navigate to "Generate Menu" screen
3. Click "Generate Menu" button
4. A random food from your list should appear
5. Click "Generate Again" to get a different random food
6. Navigate to "Food List" to see all your foods

---

## 🔧 Installation Requirements

Make sure you have React Navigation installed:

```bash
npm install @react-navigation/native @react-navigation/native-stack
npx expo install react-native-screens react-native-safe-area-context
```

---

**Note:** This implementation uses React Context for state management. For larger apps, you might want to consider Redux or Zustand, but Context is perfect for this use case and great for learning!




