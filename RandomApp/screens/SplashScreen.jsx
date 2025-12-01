import { View, StyleSheet, Animated } from 'react-native';
import { COLORS } from '../utils/colors';
import { useEffect, useRef } from 'react';
import LottieView from 'lottie-react-native';
import splashScreenAnimation from '../assets/Morphing.json';




export default function SplashScreen({onFinish}) {
    const animationRef = useRef(new Animated.Value(1)).current;

    useEffect(() => {
        const timer = setTimeout(() => {
            Animated.timing(animationRef, {
                toValue: 0,
                duration: 550,
                useNativeDriver: true,
            }).start(() => {
                onFinish();
            });
        }, 5000);
        return () => clearTimeout(timer);
    }, [onFinish]);
    
    return (
        <Animated.View style={[styles.container, { opacity: animationRef }]}>
            <LottieView source={splashScreenAnimation} 
            style={styles.animation} 
            autoPlay 
            loop={false}
            resizeMode="cover"
            />
        </Animated.View>
    );
}
const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: COLORS.mustardYellow,
    },
    animation: {
        width: 300,
        height: 300,
    },
});