import { StyleSheet, Text, View, TouchableOpacity, Alert, TextInput, ActivityIndicator, ScrollView } from 'react-native';
import { useState, useEffect } from 'react';
import { getSuggestedFoodByFavorites, getSuggestedFoodByCriteria, addFood } from '../utils/api';
import { COLORS } from '../utils/colors';

const Suggestions = ({ navigation }) => {
    const [suggestedFood, setSuggestedFood] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [criteria, setCriteria] = useState('');
    const [mode, setMode] = useState('favorites');

//function to get suggestion by favorites
    const handleGetSuggestionByFavorites = async () => {
        setIsLoading(true);
        //call api to get suggestion by favorites
        const result = await getSuggestedFoodByFavorites();
        setIsLoading(false);

        //handle 3 cases: success, needs reregistration, error
        if(result.success){
            setSuggestedFood(result.data);
        } else if(result.needsReregistration){
            Alert.alert("Session Expired", "Your device registration has expired. Please restart the app to register again.");
        } else {
            Alert.alert("Error", result.error || "Could not load suggestion by favorites");
        }
    };

//function to get suggestion by criteria
    const handleGetSuggestionByCriteria = async () => {
        if (criteria.trim() === "") {  // ✅ Check if empty!
            Alert.alert("Please enter criteria", "For example: vegan, spicy, thai");
            return;
        }
        setIsLoading(true);
        //call api to get suggestion by criteria
        const result = await getSuggestedFoodByCriteria(criteria);
        setIsLoading(false);

        //handle 3 cases: success, needs reregistration, error
        if(result.success){
            setSuggestedFood(result.data);
        } else if(result.needsReregistration){
            Alert.alert("Session Expired", "Your device registration has expired. Please restart the app to register again.");
        } else {
            Alert.alert("Error", result.error || "Could not load suggestion by criteria");
        }
    };
    const handleAddFood = async () => {

        const foodName = suggestedFood.SuggestedFood;
        
        setIsLoading(true);
        //call api to add food
        const result = await addFood(foodName);
        setIsLoading(false);
        //handle the result with 3 cases: success, needs reregistration, error
        if(result.success){
            Alert.alert("Success", "Food added to list");
        } else if(result.needsReregistration){
            Alert.alert("Session Expired", "Your device registration has expired. Please restart the app to register again.");
        } else {
            Alert.alert("Error", result.error || "Could not add food to list");
        }
    }
    const handleGetAnotherSuggestion = async () => {
        if (mode === 'favorites') {
            handleGetSuggestionByFavorites();
        } else {
            handleGetSuggestionByCriteria();
        }
    }
    const handleGetRecipe = async () => {
        if (suggestedFood && suggestedFood.Recipe) {
            navigation.navigate('RecipeDetail', { foodName: suggestedFood.SuggestedFood, recipe: suggestedFood.Recipe });
        } else {
            Alert.alert("Error", "No recipe found for this food");
        }
    }

    return (
        <ScrollView style={styles.container}>
            {/* Title */}
            <Text style={styles.title}>Suggestions</Text>
            {/* Tab Buttons - Switch between modes*/}
            <View style={styles.tabButtons}>
                <TouchableOpacity style={[styles.tabButton, mode === 'favorites' && {backgroundColor: '#4CAF50'} ]} onPress={() => setMode('favorites')}>
                    <Text style={styles.tabButtonText}>Favorites</Text>
                </TouchableOpacity>
                <TouchableOpacity style={[styles.tabButton, mode === 'criteria' && {backgroundColor: '#4CAF50'}]} onPress={() => setMode('criteria')}>
                    <Text style={styles.tabButtonText}>Criteria</Text>
                </TouchableOpacity>
            </View>
            {/* mode 1 : favorites */}
            {mode === 'favorites' && (
                <View style={styles.favoritesContainer}>
                    <TouchableOpacity style={styles.button} onPress={handleGetSuggestionByFavorites}>
                        <Text style={styles.buttonText}>Get Suggestion</Text>
                    </TouchableOpacity>
                </View>
            )}
            {/* mode 2 : criteria */}
            {mode === 'criteria' && (
                <View style={styles.criteriaContainer}>
                    <Text style={styles.subtitle}>What are you craving?</Text>
                     <TextInput 
                        style={styles.input}
                        value={criteria}
                        onChangeText={setCriteria}
                        placeholder="e.g., vegan, spicy, thai"
                        placeholderTextColor="#999"
                    />
                    <TouchableOpacity style={styles.button} onPress={handleGetSuggestionByCriteria}>
                        <Text style={styles.buttonText}>Find Food</Text>
                    </TouchableOpacity>
                </View>
            )}
             {/* Loading Indicator */}
             {isLoading && <ActivityIndicator size="large" color="#0000ff" />}
             {/* Suggestion Display */}
             {suggestedFood && !isLoading && (
                    <View style={styles.suggestionCard}>
                    <Text style={styles.foodName}>{suggestedFood.SuggestedFood}</Text>
                    <Text style={styles.reason}>{suggestedFood.Reason}</Text>
                    
                    <TouchableOpacity style={[styles.button, {backgroundColor: COLORS.mustardYellow, width: '100%'}]} onPress={handleAddFood}>
                        <Text style={styles.buttonText}>Add to My Foods</Text>
                    </TouchableOpacity>
                    
                    <TouchableOpacity style={[styles.button, {backgroundColor: COLORS.mustardYellow, width: '100%', marginTop: 5}]} onPress={handleGetAnotherSuggestion}>
                        <Text style={styles.buttonText}>Get Another Suggestion</Text>
                    </TouchableOpacity>
                    <TouchableOpacity style={[styles.button, {backgroundColor: COLORS.mustardYellow, width: '100%', marginTop: 5}]} onPress={handleGetRecipe}>
                        <Text style={styles.buttonText}>Get Recipe</Text>
                    </TouchableOpacity>
                    </View>
                )}
        </ScrollView>
    )
}

export default Suggestions;

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
        color: COLORS.textDark,
    },
    tabButtons: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 20,
    },
    tabButton: {
        padding: 10,
        borderRadius: 5,
        backgroundColor: COLORS.mustardYellow,
        marginVertical: 10,
        shadowColor: COLORS.shadow,
    },
    tabButtonText: {
        color: COLORS.textDark,
        fontSize: 16,
        fontWeight: 'bold',
        textAlign: 'center',
    },
    favoritesContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    criteriaContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    suggestionCard: {
        padding: 20,
        borderRadius: 10,
        backgroundColor: COLORS.cardBackground,
        marginBottom: 20,
        shadowColor: COLORS.shadow,
        borderWidth: 2,
        borderColor: COLORS.border,
    },
    foodName: {
        fontSize: 16,
        fontWeight: 'bold',
        color: COLORS.textDark,
    },
    reason: {
        fontSize: 14,
        color: COLORS.textMedium,
        marginBottom: 10,
    },
    button: {
        padding: 10,
        borderRadius: 5,
        backgroundColor: COLORS.coral,
        marginVertical: 10,
        width: '50%',
        alignItems: 'center',
        justifyContent: 'center',

    },
    buttonText: {
        color: COLORS.textDark,
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
    inputPlaceholder: {
        fontSize: 16,
        fontWeight: 'bold',
        textAlign: 'center',
        color: COLORS.textMedium,
    },
    subtitle: {
        fontSize: 16,
        color: COLORS.textMedium,
        marginBottom: 10,
        textAlign: 'center',
    },
});