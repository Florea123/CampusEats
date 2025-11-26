import { useEffect, useState } from 'react'
import { LoyaltyApi } from '../services/api'

export function useLoyaltyPoints(shouldFetch: boolean = true) {
    const [points, setPoints] = useState<number | null>(null)
    const [loading, setLoading] = useState(shouldFetch)

    const loadPoints = async () => {
        if (!shouldFetch) {
            setPoints(null)
            setLoading(false)
            return
        }

        try {
            setLoading(true)
            const account = await LoyaltyApi.getAccount()
            setPoints(account.points)
        } catch (err) {
            console.error('Failed to load loyalty points:', err)
            setPoints(0)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadPoints()
    }, [shouldFetch])

    return { points, loading, refresh: loadPoints }
}