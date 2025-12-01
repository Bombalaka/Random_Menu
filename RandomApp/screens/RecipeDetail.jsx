import { StyleSheet, Text, View, TouchableOpacity, Alert, TextInput, ActivityIndicator, FlatList, ScrollView } from 'react-native';
import { COLORS } from '../utils/colors';
import { SafeAreaView } from 'react-native-safe-area-context';


const RecipeDetail = ({ navigation, route }) => {

const { foodName, recipe } = route.params;

return (
    <SafeAreaView style={styles.container}>
        <ScrollView>
        {/* Title */}
        <Text style={styles.title}>{foodName}</Text>
        <Text style={styles.subtitle}>{recipe.Title}</Text>

        {/* Info */}
        <View style={styles.infoContainer}>
                <Text>⏱{recipe.PrepTime}</Text>
                <Text>👥{recipe.Servings}</Text>
                <Text>💪{recipe.Difficulty}</Text>
        </View>

        {/* Description */}
        <Text style={styles.description}>{recipe.Description}</Text>

        {/* Ingredients */}
        <Text style={styles.ingredients}>Ingredients:</Text>
        <FlatList
                data={recipe.Ingredients}
                scrollEnabled={false}
                keyExtractor={(item, index) => index.toString()}
                renderItem={({ item, index }) => (
                    <Text style={styles.ingredientItem}>{item}</Text>
                )}
            />

        {/* Instructions */}
        <Text style={styles.instructions}>Instructions:</Text>
        <FlatList
                data={recipe.Instructions}
                scrollEnabled={false}
                keyExtractor={(item, index) => index.toString()}
                renderItem={({ item, index }) => (
                    <View style={styles.instructionContainer}>
                        <Text style={styles.instructionItem}>{index + 1}. {item}</Text>
                        <Text style={styles.instructionStep}>{item}</Text>
                    </View>
                )}
            />
        </ScrollView>
       {/* Back Button */}
       <TouchableOpacity style={styles.button} onPress={() => navigation.goBack()}>
        <Text style={styles.buttonText}>Back</Text>
       </TouchableOpacity>

    </SafeAreaView>
);
};


export default RecipeDetail;

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
        padding: 20,
    },
    title: {
        fontSize: 24,
        fontWeight: 'bold',
        marginBottom: 10,
        color: COLORS.textDark,
    },
    subtitle: {
        fontSize: 16,
        fontWeight: 'bold',
        marginBottom: 10,
        color: COLORS.textDark,
    },
    infoContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 10,
        color: COLORS.textDark,
    },
    infoText: {
        fontSize: 16,
        fontWeight: 'bold',
        marginBottom: 10,
        color: COLORS.textDark,
    },
    description: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
    },
    ingredients: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
        fontWeight: 'bold',
    },
    instructions: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
        fontWeight: 'bold',
    },
    instructionContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 10,
        color: COLORS.textDark,
    },
    instructionItem: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
    },
    instructionStep: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
    },
    button: {
        padding: 10,
        borderRadius: 10,
        backgroundColor: COLORS.primary,
        marginVertical: 10,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        marginBottom: 10,
        marginHorizontal: 10,
        
    },
    buttonText: {
        color: COLORS.textDark,
        fontSize: 16,
        fontWeight: 'bold',
        textAlign: 'center',
    },
    ingredientItem: {
        fontSize: 16,
        marginBottom: 10,
        color: COLORS.textDark,
    },
    ingredientsList: {
        marginBottom: 10,
        color: COLORS.textDark,
    },
    instructionsList: {
        marginBottom: 10,
        color: COLORS.textDark,
    },

});