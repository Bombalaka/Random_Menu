import { StyleSheet, Text, View, TouchableOpacity, Alert, Image } from 'react-native';
import { useState, useEffect, useCallback } from 'react';
import { useNavigation } from '@react-navigation/native';
import { useFocusEffect } from '@react-navigation/native';
import { generateMenu, getFoods, getFoodImage } from '../utils/api';
import { COLORS } from '../utils/colors';
import LottieView from 'lottie-react-native';
import Animated, {FadeInDown} from 'react-native-reanimated';
import generateMenuAnimation from '../assets/food around the city.json';


const GenerateMenu = () => {
  const navigation = useNavigation(); // Get navigation object using hook
  //set state for menu == null
  const [menu, setMenu] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [foodCount, setFoodCount] = useState(0);
  const [foodImage, setFoodImage] = useState(null);
  const [CheckFoodCount, setCheckFoodCount] = useState(true);


  useFocusEffect(
    useCallback(() => {
    const loadFoodCount = async () => {
      const result = await getFoods(); // Use imported getFoods
      if(result.success) {
        const foods = result.data?.foods || [];
        setFoodCount(foods.length);
      }
      setCheckFoodCount(false);
    };
    loadFoodCount();
    return () => {
      
    };
  }, []));

  //function to generate menu and check if user has food  
  const HandleGenerateMenu = async () => {
    setIsLoading(true);
    //crdeate timer 1.5 seconds
    const timer = new Promise((resolve) => setTimeout(resolve, 1500));
    // call api to generate menu
    const result = await generateMenu();
    //wait for timer to finish
    const[_,results] = await Promise.all([timer, results]);

    //handle the result
    if(result.success){
      // Backend returns { menu: [...] }, so we need to access result.data.menu
      // This keeps frontend and backend separate - if backend changes, we only change this line
      const menuData = result.data?.menu?.FoodName || null;
      console.log("🔍 Menu data:", menuData);
      setMenu(menuData);
      //get food image from themealDB.com
      const foodImage = await getFoodImage(menuData);
      setFoodImage(foodImage);
      console.log("🔍 Food image:", foodImage);
  } else if(result.needsReregistration){
      // Session expired - tell user to restart
      Alert.alert(
          "Session Expired",
          "Your device registration has expired. Please restart the app to register again.",
          [{ text: "OK" }]
      );
  } else {
      // Other errors
      Alert.alert("Error", result.error || "Could not load menu");
  }
  setIsLoading(false);
  
};

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Generate Random Menu</Text>
      
      {/* Show how many foods are available */}
      {menu == null && !isLoading && !CheckFoodCount && (
        <Animated.View style={styles.subtitle} entering={FadeInDown.duration(600).springify()}>
          <Text style={styles.subtitle}>
            {foodCount === 0 ? "No foods available. Add some foods first!" : `${foodCount} food${foodCount !== 1 ? 's' : ''} available`}
          </Text>
        </Animated.View>
      )}
      {/*loading state*/}
      {isLoading ? (
        <View style={styles.loadingContainer}>
          <LottieView source={generateMenuAnimation} autoPlay loop style={styles.loadingAnimation} />
          <Text style={styles.loadingText}>Generating menu...</Text>
        </View>
      ) : (
        //THE RESULT (Animated "Pop" Reveal)
        menu != null && (
          <Animated.View style={styles.menuContainer} entering={FadeInDown.duration(600).springify()}>
            <Text style={styles.menuTitle}>Your Menu:</Text>
            {foodImage && (
              <Image source={{ uri: foodImage }} style={styles.foodImage} />
            )}
            <Text style={styles.menuFood}>{menu|| 'No menu found'}</Text>
          </Animated.View>
        )
      )}
      
      
      {/*if not generated yet show button generate menu*/}
      {menu == null && (
        <TouchableOpacity style={styles.button} onPress={HandleGenerateMenu}>
          <Text style={styles.buttonText}>Generate Menu</Text>
        </TouchableOpacity>
      )}
      
      {/*generate again button*/}
      {menu != null && !isLoading && (
        <TouchableOpacity style={styles.button} onPress={HandleGenerateMenu}>
          <Text style={styles.buttonText}>Generate Again</Text>
        </TouchableOpacity>
      )}
      {/*ONLY SHOW AFTER MENU IS GENERATED*/}
      {menu != null && !isLoading && (
        <View>
          <Text style={styles.subtitle}>Want more variety? Get more suggestions from AI!</Text>
          {/*AI suggestions button link to suggestions screen*/}
          <TouchableOpacity style={styles.button} onPress={() => navigation.navigate('Suggestions')}>
            <Text style={styles.buttonText}>Get More Suggestions</Text>
          </TouchableOpacity>
        </View>
      )}

    </View>
  )
}

export default GenerateMenu;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 10,
    color: COLORS.textDark,
  },
  subtitle: {
    fontSize: 14,
    color: COLORS.textMedium,
    marginBottom: 30,
    textAlign: 'center',
    shadowColor: COLORS.shadow,
    marginTop: 20,
  },
  button: {
    padding: 15,
    borderRadius: 5,
    backgroundColor: COLORS.mustardYellow,
    marginVertical: 10,
    minWidth: 200,
    shadowColor: COLORS.shadow,
    elevation: 5
  },
  buttonText: {
    color: COLORS.textMedium,
    fontSize: 16,
    fontWeight: 'bold',
    textAlign: 'center',
  },
  menuContainer: {
    marginVertical: 30,
    padding: 20,
    backgroundColor: COLORS.cardBackground,
    borderRadius: 10,
    alignItems: 'center',
    shadowColor: COLORS.shadow,
    borderWidth: 1,
    borderColor: COLORS.border,
    elevation: 20,
   
  },
  menuTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 10,
    color: COLORS.textDark,
  },
  menuFood: {
    fontSize: 22,
    fontWeight: 'bold',
    color: COLORS.textDark,
  },
  loading: {
    fontSize: 18,
    color: COLORS.textMedium,
    marginTop: 20,
  },
  foodImage: {
    width: 200,
    height: 200,
    borderRadius: 5,
    marginBottom: 10,
  },
  loadingContainer: {
    marginVertical: 30,
    padding: 20,
    backgroundColor: 'transparent',
    alignItems: 'center',
    
  },
  loadingAnimation: {
    width: 200,
    height: 200,
  },
  loadingText: {
    fontSize: 16,
    color: COLORS.textMedium,
    marginTop: 10,
    textAlign: 'center',
  },

});

