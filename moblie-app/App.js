import { StatusBar } from 'expo-status-bar';
import { StyleSheet, Text, View, TouchableOpacity, Image, ActivityIndicator } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
//import AsyncStorage
import AsyncStorage from '@react-native-async-storage/async-storage';
import { useState, useEffect } from 'react';
import 'react-native-get-random-values';
//import home screens 
import AddFood from './screens/AddFood';
import GenerateMenu from './screens/GenerateMenu';
import FoodList from './screens/FoodList';
import WelcomeScreen from './screens/Welcome';
import { getFoods, getSuggestedFoodByFavorites, getSuggestedFoodByCriteria, editFood } from './utils/api';
import Suggestions from './screens/Suggestions';
import EditFood from './screens/EditFood';
import RecipeDetail from './screens/RecipeDetail';
import { COLORS } from './utils/colors';
import SplashScreen from './screens/SplashScreen';



const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

// This is the Home tab content - just shows logo and welcome text
function Home({ navigation }) {
  const [foodCount, setFoodCount] = useState(0);
  useEffect(() => {
    getFoods().then(result => {
      if(result.success) {
        setFoodCount(result.data.foods.length);
      }
    });
  }, []);
  return (
    <View style={styles.container}>
      <Image source={require('./assets/logo-cat.png')} style={styles.logo} />
      <Text style={styles.title}>Welcom to the Random Menu App!</Text>
      <Text style={{ marginBottom: 20, fontWeight: 'italic', color: COLORS.textMedium }}>Hungry? Let fate decide.</Text>

      <View style={styles.statsContainer}>
        <View style={styles.statBadge}>
          <Text style={styles.statEmoji}>🥗</Text>
          <Text style={styles.statText}>{foodCount} Foods Saved</Text>
        </View>
      </View>
    </View>
    
    
  )
}

// This is the Tab Navigator - it wraps all your bottom tabs
// It's OUTSIDE the Home component, so no circular reference!
function TabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={{
        tabBarStyle: { backgroundColor: COLORS.background, height: 100,},
        tabBarActiveTintColor: COLORS.activeTab,
        tabBarInactiveTintColor: COLORS.inactiveTab,
        headerShown: false,
      }}
    >
      <Tab.Screen 
        name="Home" 
        component={Home}
        options={{
          tabBarIcon: ({ color }) => <Text style={{fontSize: 24}}>🏠</Text>,
        }}
      />
      <Tab.Screen 
        name="FoodList" 
        component={FoodList}
        options={{
          title: 'Foods',
          tabBarIcon: ({ color }) => <Text style={{fontSize: 24}}>📋</Text>,
        }}
      />
      <Tab.Screen 
        name="GenerateMenu" 
        component={GenerateMenu}
        options={{
          title: 'Generate',
          tabBarIcon: ({ color }) => <Text style={{fontSize: 24}}>🎲</Text>,
        }}
      />
    </Tab.Navigator>
  )
}


//check registration and show welcome screen if not registered
export default function App() {
  const [isRegistered, setIsRegistered] = useState(null);
  const [isSplashScreen, setIsSplashScreen] = useState(true);
  useEffect(() => {
    checkRegistration();
  }, []);

  const checkRegistration = async () => {
    const registered = await AsyncStorage.getItem('isRegistered');
    
    if (registered === 'true') {
        // Quick check: Try to get foods
        // // Try to get data from backend
        const result = await getFoods();
        
        if (result.needsReregistration) {
            // Clear and show Welcome
            await AsyncStorage.removeItem('isRegistered');
            setIsRegistered(false); // Show Welcome screen
            return;
        }
    }
    
    setIsRegistered(registered === 'true'); // Show Home screen
  };

  //This function runs when the Lottie animation timer finishes
  // It's defined here (at component level) so it can be passed to SplashScreen
  const handleSplashFinish = () => {
    setIsSplashScreen(false);
  };


  //if not registered, show welcome screen
  // If the animation is still playing OR we are still checking the database...
  // ... Keep showing the Splash Screen
  if(isSplashScreen || isRegistered === null){
    return (<SplashScreen onFinish={handleSplashFinish} />);
  }

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ animation: 'slide_from_right' }}>
        {!isRegistered ? (
          //not registered? show welcome screen
          <Stack.Screen 
            name="Welcome" 
            options={{ headerShown: false }}
          >
            {(props) => (
              <WelcomeScreen {...props} 
              onRegisterSuccess={() => {
                checkRegistration();
              }} 
              />
            )}
          </Stack.Screen>
          
        ) : (
          //resgistered? show home screen with tabs
          <>
          {/* show home screen with tabs */}
            <Stack.Screen 
              name="MainTabs" 
              component={TabNavigator}
              options={{ headerShown: false}}
            />
            <Stack.Screen 
              name="AddFood" 
              component={AddFood}
              options={{ title: 'ADD FOOD' , headerStyle: {
                backgroundColor: COLORS.coral,  // Different color for this screen
              }, headerTintColor: COLORS.textDark, headerTitleStyle: {
                fontWeight: 'bold',
              }, animation: 'slide_from_bottom', presentation: 'modal' }}
              
            />
            <Stack.Screen 
              name="GenerateMenu" 
              component={GenerateMenu}
              options={{ title: 'headerShown: false' }}
            />
            <Stack.Screen 
              name="FoodList" 
              component={FoodList}
              options={{ title: 'You Foods List', headerShown: false }} 
            />
            <Stack.Screen 
              name="Suggestions" 
              component={Suggestions}
              options={{ title: 'Food Suggestions' , headerStyle: {
                backgroundColor: COLORS.coral,  // Different color for this screen
              }, headerTintColor: COLORS.textDark, headerTitleStyle: {
                fontWeight: 'bold',
              }}}
            />
            <Stack.Screen 
              name="EditFood" 
              component={EditFood}
              options={{ title: 'Edit Food', headerShown: false, animation: 'slide_from_bottom', presentation: 'modal' }} 
            />
            <Stack.Screen 
              name="RecipeDetail" 
              component={RecipeDetail}
              options={{ title: 'Recipe Detail' , headerShown: false }}
            />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
    alignItems: 'center',
    justifyContent: 'center',
    
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 20,
    color: COLORS.textDark,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    gap: 5,
  },
  button: {
    padding: 10,
    borderRadius: 5,
    backgroundColor: COLORS.primary,
    marginVertical: 20,
    paddingHorizontal: 25,
    paddingVertical: 15,
    width: '30%',
    shadowColor: COLORS.shadow,
  },
  buttonText: {
    color: COLORS.textDark,
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
    },
    logo: {
        width: 250,
        height: 250,
        borderRadius: 30,
        marginBottom: 20,
    },
    statsContainer: {
      flexDirection: 'row',
      justifyContent: 'center',
      gap: 10,
    },
    statBadge: {
      flexDirection: 'row',
      backgroundColor: COLORS.cardBackground, // Use a light background (like white)
      paddingVertical: 10,
      paddingHorizontal: 20,
      borderRadius: 25, // Fully rounded "Pill" shape
      borderWidth: 1,
      borderColor: COLORS.border,
      alignItems: 'center',
      gap: 8,
    },
    statEmoji: {
      fontSize: 24,
    },
    statText: {
      fontSize: 16,
      fontWeight: '600',
      color: COLORS.textDark,
    },
});
