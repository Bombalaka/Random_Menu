import { StyleSheet, Text, View, TouchableOpacity, Alert, TextInput, ActivityIndicator } from 'react-native';
import { useState } from 'react';
import { addFood } from '../utils/api';
import { COLORS } from '../utils/colors';


// AddFood component needs to receive 'navigation' as a parameter
// This allows it to navigate to other screens
const AddFood = ({ navigation }) => {
    //algorithem for add food and update food(memory hook)
    const [foodName, setFoodName] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const handleAddFood = async () => {
        if (foodName.trim() === "") {
            Alert.alert("Please enter a food name");
            return;
        }

        setIsLoading(true);
        //call api to add food
        const result = await addFood(foodName);
        setIsLoading(false);

        //handle the result
        if (result.success) {
            // ✅ SUCCESS!
            Alert.alert("Success", `Added ${foodName} to the database!`);
            setFoodName(""); // Clear input
        } else if (result.needsReregistration) {
            // ❌ Not registered error
            Alert.alert(
                "Registration Required",
                "Your device is not registered. Please restart the app to register again.",
                [{ text: "OK" }]
            );
        } else {
            // ❌ Other errors
            Alert.alert("Error", result.error);
        }
    };




    return (
        <View style={styles.container}>
            <Text style={styles.title}>Add Food Name</Text>
            {/*the feedback loop for the food name*/}
            <TextInput style={styles.input} placeholder="Enter Food Name" value={foodName} onChangeText={setFoodName} />
            {/*the button for add food and clear food list*/}

            <TouchableOpacity
                style={[styles.button, isLoading ? { opacity: 0.5 } : {}]}
                onPress={handleAddFood}
                disabled={isLoading}
            >
                {isLoading ? (
                    <ActivityIndicator size="large" color="#fff" />
                ) : (
                    <Text style={styles.buttonText}>Add Food</Text>
                )}
            </TouchableOpacity>



        </View>
    );

}
export default AddFood;

const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: COLORS.background,
    },
    title: {
        fontSize: 20,
        fontWeight: 'bold',
        color: COLORS.textDark,
    },
    button: {
        padding: 10,
        borderRadius: 5,
        backgroundColor: COLORS.primary,
        marginVertical: 10,
    },
    buttonText: {
        color: COLORS.textMedium,
        fontSize: 16,
        fontWeight: 'bold',
        textAlign: 'center',
    },
    input: {
        width: '80%',
        height: 40,
        borderColor: COLORS.border,
        borderWidth: 1,
        marginBottom: 10,
        paddingHorizontal: 10,
        color: COLORS.textDark,
        backgroundColor: COLORS.cardBackground,
        shadowColor: COLORS.shadow,
    },
    buttonContainer: {
        flexDirection: 'row',
        justifyContent: 'center',
        width: '100%',

    },
    foodName: {
        fontSize: 16,
        fontWeight: 'bold',
        color: COLORS.textDark,
        textAlign: 'center',
        marginVertical: 10,

    },
});
