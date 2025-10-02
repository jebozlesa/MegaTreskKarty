// =====================================
// PRIDAJTE DO PLAYFAB CLOUDSCRIPT
// =====================================

// Pridajte túto funkciu do vášho PlayFab CloudScript editora

handlers.markRoomAsCompleted = function (args, context) {
    var roomCode = args.roomCode;
    var playerId = args.playerId;
    
    if (!roomCode || !playerId) {
        return { success: false, message: "roomCode and playerId are required" };
    }
    
    // URL vašej Vercel funkcie (ZMEŇTE NA SPRÁVNU URL!)
    var vercelFunctionUrl = "https://YOUR_VERCEL_URL/api/markRoomAsCompleted";
    
    var requestBody = {
        roomCode: roomCode,
        playerId: playerId
    };
    
    try {
        var httpResponse = http.request(vercelFunctionUrl, "POST", JSON.stringify(requestBody), "application/json", {});
        
        if (httpResponse.status === 200) {
            var responseData = JSON.parse(httpResponse.body);
            return responseData;
        } else {
            return { 
                success: false, 
                message: "Vercel function call failed with status: " + httpResponse.status,
                details: httpResponse.body
            };
        }
    } catch (error) {
        return { 
            success: false, 
            message: "Error calling Vercel function: " + error.message 
        };
    }
};