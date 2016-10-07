
public enum MessageType : ushort {
	Connect, // Client side
	Disconnect, // Client side
	ConnectResponse, // Server side
	UserConnected, // Server side
	DisconnectResponse, // Server side
	UserDisconnected, // Server side

	Position,
	Rotation,
	Scale,
	Text
}

// Connect to server
// Disconnect from server

// Server connect response
// Server disconnect response

// Server user connected
// Server user left

// Position data
// Rotation data
// Scale data
// Text data