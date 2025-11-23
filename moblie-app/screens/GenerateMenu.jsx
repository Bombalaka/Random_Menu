import { StyleSheet, Text, View, TouchableOpacity, Alert } from 'react-native';
import { useState, useEffect } from 'react';
import { useNavigation } from '@react-navigation/native';
import { generateMenu, getFoods } from '../utils/api';
import { COLORS } from '../utils/colors';


const GenerateMenu = () => {
  const navigation = useNavigation(); // Get navigation object using hook
  //set state for menu == null
  const [menu, setMenu] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [foodCount, setFoodCount] = useState(0);

  useEffect(() => {
    const loadFoodCount = async () => {
      const result = await getFoods(); // Use imported getFoods
      if(result.success) {
        const foods = result.data?.foods || [];
        setFoodCount(foods.length);
      }
    };
    loadFoodCount();
  }, []);

  //function to generate menu and check if user has food  
  const HandleGenerateMenu = async () => {
    setIsLoading(true);
    // call api to generate menu
    const result = await generateMenu();
    setIsLoading(false);

    //handle the result
    if(result.success){
      // Backend returns { menu: [...] }, so we need to access result.data.menu
      // This keeps frontend and backend separate - if backend changes, we only change this line
      const menuData = result.data?.menu?.FoodName || null;
      setMenu(menuData);
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
};

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Generate Random Menu</Text>
      
      {/* Show how many foods are available */}
      {menu == null ? (
        <Text style={styles.subtitle}>No menu found</Text>
      ) : (
        <Text style={styles.subtitle}>
          {foodCount} food{foodCount !== 1 ? 's' : ''} available
        </Text>
      )}
      
      {/*if menu is existing, show it*/}
      {menu != null && (
        <View style={styles.menuContainer}>
          <Text style={styles.menuTitle}>Your Random Menu:</Text>
          <Text style={styles.menuFood}>{menu|| 'No menu found'}</Text>
        </View>
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
      <Text style={styles.subtitle}>Want more variety? Get more suggestions from AI!</Text>
      {/*AI suggestions button link to suggestions screen*/}
      <TouchableOpacity style={styles.button} onPress={() => navigation.navigate('Suggestions')}>
        <Text style={styles.buttonText}>Get More Suggestions</Text>
      </TouchableOpacity>
      
      {/*if menu is loading show loading*/}
      {isLoading && <Text style={styles.loading}>Loading...</Text>}
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

});

