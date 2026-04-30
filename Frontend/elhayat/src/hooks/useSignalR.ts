import { useEffect } from 'react'
import { signalRService } from '../services/signalrService'

/**
 * Hook to manage SignalR connection lifecycle and event listeners.
 */
export function useSignalR(
  events: Array<{ name: string; handler: (...args: any[]) => void }>
) {
  useEffect(() => {
    let isMounted = true

    const connectAndListen = async () => {
      await signalRService.startConnection()
      
      if (!isMounted) return

      // Register event listeners
      events.forEach(({ name, handler }) => {
        signalRService.on(name, handler)
      })
    }

    connectAndListen()

    return () => {
      isMounted = false
      // Unregister event listeners
      events.forEach(({ name, handler }) => {
        signalRService.off(name, handler)
      })
      // Do not stop connection globally here if other components might use it,
      // but since it's used primarily by the Dashboard, we could leave it open 
      // or implement a ref-counted connection manager.
    }
  }, [events])
}
