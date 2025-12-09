

export const getGreeting = () => {
    const hour = new Date().getHours(); // Returns 0-23 (24-hour format)
    
    // Morning: 6 AM to 11:59 AM (hours 6-11)
    if (hour >= 6 && hour < 12) {
        return 'Good Morning';
    }
    
    // Afternoon: 12 PM (noon) to 5:59 PM (hours 12-17)
    if (hour >= 12 && hour < 18) {
        return 'Good Afternoon';
    }
    
    
    // Evening/Night: 6 PM to 5:59 AM (hours 18-23 or 0-5)
    return 'Good Evening';
};


