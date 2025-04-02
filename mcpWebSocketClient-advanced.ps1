# Advanced MCP WebSocket Client for TBD Space Game
# This script connects to the MCP Unity WebSocket server and can send various command types

param (
    [Parameter(Mandatory=$false)]
    [string]$menuPath,
    
    [Parameter(Mandatory=$false)]
    [string]$message,
    
    [Parameter(Mandatory=$false)]
    [string]$logType = "Log",
    
    [Parameter(Mandatory=$false)]
    [string]$gameObjectName,
    
    [Parameter(Mandatory=$false)]
    [string]$componentType,
    
    [Parameter(Mandatory=$false)]
    [string]$propertyName,
    
    [Parameter(Mandatory=$false)]
    [string]$propertyValue,
    
    [Parameter(Mandatory=$false)]
    [string]$commandType = "menu_item",
    
    [Parameter(Mandatory=$false)]
    [string]$serverUrl = "ws://localhost:8001/McpUnity"
)

# Load required .NET assemblies
Add-Type -AssemblyName System.Net.WebSockets
Add-Type -AssemblyName System.Threading.Tasks

# Generate a unique ID for this request
$requestId = [guid]::NewGuid().ToString()

# Function to create a connection to WebSocket
function Connect-ToWebSocket {
    param (
        [string]$url
    )
    
    Write-Host "Connecting to WebSocket at $url..."
    
    try {
        $clientWebSocket = New-Object System.Net.WebSockets.ClientWebSocket
        $connection = $clientWebSocket.ConnectAsync([System.Uri]$url, [System.Threading.CancellationToken]::None)
        $connection.Wait()
        
        if ($clientWebSocket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
            Write-Host "True"
            Write-Host "WebSocket connected successfully"
            return $clientWebSocket
        } else {
            Write-Host "False"
            Write-Host "Failed to connect to WebSocket. State: $($clientWebSocket.State)"
            return $null
        }
    } catch {
        Write-Host "False"
        Write-Host "Error connecting to WebSocket: $_"
        return $null
    }
}

# Function to send a message through WebSocket
function Send-WebSocketMessage {
    param (
        [System.Net.WebSockets.ClientWebSocket]$webSocket,
        [string]$message
    )
    
    try {
        $utf8 = [System.Text.Encoding]::UTF8
        $bytes = $utf8.GetBytes($message)
        $arraySegment = New-Object System.ArraySegment[byte] -ArgumentList @(,$bytes)
        
        $sendTask = $webSocket.SendAsync(
            $arraySegment,
            [System.Net.WebSockets.WebSocketMessageType]::Text,
            $true,
            [System.Threading.CancellationToken]::None
        )
        $sendTask.Wait()
        
        Write-Host "True"
        Write-Host "Request sent successfully"
        return $true
    } catch {
        Write-Host "False"
        Write-Host "Error sending message: $_"
        return $false
    }
}

# Function to receive a message from WebSocket
function Receive-WebSocketMessage {
    param (
        [System.Net.WebSockets.ClientWebSocket]$webSocket
    )
    
    try {
        $buffer = New-Object byte[] 8192
        $arraySegment = New-Object System.ArraySegment[byte] -ArgumentList @(,$buffer)
        
        $receiveTask = $webSocket.ReceiveAsync($arraySegment, [System.Threading.CancellationToken]::None)
        $receiveTask.Wait()
        
        $result = $receiveTask.Result
        if ($result.MessageType -eq [System.Net.WebSockets.WebSocketMessageType]::Text) {
            $message = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $result.Count)
            return $message
        } else {
            Write-Host "Received non-text message type: $($result.MessageType)"
            return $null
        }
    } catch {
        Write-Host "Error receiving message: $_"
        return $null
    }
}

# Function to close the WebSocket connection
function Close-WebSocketConnection {
    param (
        [System.Net.WebSockets.ClientWebSocket]$webSocket
    )
    
    if ($webSocket -ne $null -and $webSocket.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
        try {
            $closeTask = $webSocket.CloseAsync(
                [System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure,
                "Closing connection",
                [System.Threading.CancellationToken]::None
            )
            $closeTask.Wait()
            Write-Host "True"
            Write-Host "WebSocket connection closed"
        } catch {
            Write-Host "Error closing WebSocket connection: $_"
        }
    }
}

# Function to prepare request payload based on command type
function Get-RequestPayload {
    param (
        [string]$commandType
    )
    
    $params = @{}
    
    switch ($commandType) {
        "menu_item" {
            if (-not [string]::IsNullOrEmpty($menuPath)) {
                $params["menuPath"] = $menuPath
                $method = "execute_menu_item"
                Write-Host "Performing operation: execute_menu_item with target: $menuPath"
            } else {
                Write-Host "Error: menuPath is required for menu_item command type"
                return $null
            }
        }
        "message" {
            if (-not [string]::IsNullOrEmpty($message)) {
                $params["message"] = $message
                $params["logType"] = $logType
                $method = "notify_message"
                Write-Host "Performing operation: notify_message with message: $message"
            } else {
                Write-Host "Error: message is required for message command type"
                return $null
            }
        }
        "update_component" {
            if (-not [string]::IsNullOrEmpty($gameObjectName) -and -not [string]::IsNullOrEmpty($componentType) -and 
                -not [string]::IsNullOrEmpty($propertyName) -and -not [string]::IsNullOrEmpty($propertyValue)) {
                $params["gameObjectName"] = $gameObjectName
                $params["componentType"] = $componentType
                $params["propertyName"] = $propertyName
                $params["propertyValue"] = $propertyValue
                $method = "update_component"
                Write-Host "Performing operation: update_component on $gameObjectName.$componentType.$propertyName"
            } else {
                Write-Host "Error: gameObjectName, componentType, propertyName, and propertyValue are required for update_component command type"
                return $null
            }
        }
        "select_gameobject" {
            if (-not [string]::IsNullOrEmpty($gameObjectName)) {
                $params["gameObjectName"] = $gameObjectName
                $method = "select_gameobject"
                Write-Host "Performing operation: select_gameobject with target: $gameObjectName"
            } else {
                Write-Host "Error: gameObjectName is required for select_gameobject command type"
                return $null
            }
        }
        default {
            Write-Host "Unsupported command type: $commandType"
            return $null
        }
    }
    
    $payload = @{
        "method" = $method
        "params" = $params
        "id" = $requestId
    }
    
    return $payload | ConvertTo-Json -Compress
}

# Main execution flow
$operationDesc = switch ($commandType) {
    "menu_item" { "execute_menu_item with target: $menuPath" }
    "message" { "notify_message with message: $message" }
    "update_component" { "update_component on $gameObjectName.$componentType.$propertyName" }
    "select_gameobject" { "select_gameobject with target: $gameObjectName" }
    default { "unknown operation type: $commandType" }
}

Write-Host "Connecting to MCP Unity server and performing operation: $operationDesc"

# Connect to WebSocket
$webSocket = Connect-ToWebSocket -url $serverUrl

if ($webSocket -ne $null) {
    try {
        # Prepare the payload
        $payload = Get-RequestPayload -commandType $commandType
        
        if ($payload -ne $null) {
            # Send request
            Write-Host "Sending request: $payload"
            $success = Send-WebSocketMessage -webSocket $webSocket -message $payload
            
            if ($success) {
                # Wait for response
                Start-Sleep -Milliseconds 500
                Write-Host "Command execution initiated in Unity"
                
                # Optionally receive response if needed
                # $response = Receive-WebSocketMessage -webSocket $webSocket
                # if ($response -ne $null) {
                #     Write-Host "Received response: $response"
                # }
            }
        }
    } finally {
        # Close WebSocket connection
        Close-WebSocketConnection -webSocket $webSocket
    }
} 