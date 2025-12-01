import AsyncStorage from '@react-native-async-storage/async-storage';
import { v4 as uuidv4 } from 'uuid';




//function to get deviceId from async storage
const getDeviceId = async () => {

    let user_deviceId = await AsyncStorage.getItem('user_deviceId');
    //if user_deviceId is not found, generate a new one
    if(!user_deviceId){
        user_deviceId = uuidv4();
        await AsyncStorage.setItem('user_deviceId', user_deviceId); //save it 
    }
    return user_deviceId; //return the user_deviceId
}

//export deviceId
export default getDeviceId;