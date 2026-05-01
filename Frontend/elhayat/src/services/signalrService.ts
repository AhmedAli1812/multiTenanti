import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { getStoredToken } from '../utils/auth'

const SIGNALR_URL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL.replace('/api', '')}/hubs/notifications`
  : '/hubs/notifications'

class SignalRService {
  private connection: HubConnection | null = null
  private isConnecting = false

  public async startConnection(): Promise<void> {
    if (this.connection?.state === 'Connected' || this.isConnecting) return

    this.isConnecting = true

    try {
      const token = getStoredToken()
      if (!token) throw new Error('No auth token available for SignalR')

      this.connection = new HubConnectionBuilder()
        .withUrl(SIGNALR_URL, {
          accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build()

      await this.connection.start()
      console.log('🔗 SignalR Connected successfully!')
    } catch (error) {
      console.error('❌ SignalR Connection Error:', error)
      // Optional: implement retry logic here if automatic reconnect fails initially
    } finally {
      this.isConnecting = false
    }
  }

  public stopConnection(): void {
    if (this.connection) {
      this.connection.stop()
      this.connection = null
    }
  }

  public on(eventName: string, callback: (...args: any[]) => void): void {
    if (this.connection) {
      this.connection.on(eventName, callback)
    }
  }

  public off(eventName: string, callback: (...args: any[]) => void): void {
    if (this.connection) {
      this.connection.off(eventName, callback)
    }
  }
}

export const signalRService = new SignalRService()
