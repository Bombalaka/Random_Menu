import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert, ActivityIndicator, Image } from 'react-native';
import { registerDevice } from '../utils/api';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { COLORS } from '../utils/colors';

export default function WelcomeScreen({ navigation, onRegisterSuccess  }) {
    const [username, setUsername] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const handleRegister = async () => {
        // Validation: Backend requires 2-50 characters
        if (username.length < 2) {
            Alert.alert("Too Short", "Username must be at least 2 characters.");
            return;
        }

        setIsLoading(true);
        // Call your registerDevice API
        const result = await registerDevice(username);
        setIsLoading(false);

        if (result.success) {
            await AsyncStorage.setItem('isRegistered', 'true');

            //call back recheck registration
            if(onRegisterSuccess){
                onRegisterSuccess();
            }
        } else {
            Alert.alert("Error", "Could not register: " + result.error);
        }
    };

    return (
        <View style={styles.container}>
            <Text style={styles.title}>Welcome!</Text>
            <Text style={styles.subtitle}>Enter your name to start eating random food</Text>
            <Image source={require('../assets/logo-cat.png')} style={styles.logo} />

            <TextInput 
                style={styles.input}
                placeholder="Your Name (e.g. John Doe)"
                value={username}
                onChangeText={setUsername}
            />

            <TouchableOpacity 
                style={styles.button} 
                onPress={handleRegister}
                disabled={isLoading}
            >
                {isLoading ? (
                    <ActivityIndicator color="white" />
                ) : (
                    <Text style={styles.buttonText}>Start Eating Random Food 🚀</Text>
                )}
            </TouchableOpacity>
        </View>
    );
}

const styles = StyleSheet.create({
    container: { 
        flex: 1, 
        justifyContent: 'center', 
        alignItems: 'center', 
        padding: 20, 
        backgroundColor: COLORS.background 
    },
    title: { 
        fontSize: 32, 
        fontWeight: 'bold',
         marginBottom: 10, 
         color: COLORS.textDark },
    subtitle: { 
        fontSize: 16, 
        color: COLORS.textMedium, 
        marginBottom: 30,
        textAlign: 'center'
    },
    input: { 
        width: '100%', 
        height: 50, 
        borderWidth: 1, 
        borderColor: COLORS.border, 
        color: COLORS.textDark,
        backgroundColor: COLORS.cardBackground,
        shadowColor: COLORS.shadow,
        borderRadius: 8, 
        padding: 15, 
        fontSize: 18, 
        marginBottom: 20,
        textAlign: 'center' },
    button: { 
        width: '100%', 
        height: 50, 
        backgroundColor: COLORS.primary, 
        borderRadius: 8, 
        justifyContent: 'center', 
        alignItems: 'center' },
        shadowColor: COLORS.shadow,
    buttonText: { 
        color: COLORS.textLight,
        fontSize: 18, 
        fontWeight: 'bold',
        textAlign: 'center',
    },
    logo: { 
        width: 100, 
        height: 100, 
        marginBottom: 20,
        borderRadius: 30
    }
});