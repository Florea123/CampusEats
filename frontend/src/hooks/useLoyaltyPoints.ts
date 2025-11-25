import { useEffect, useState } from 'react'
import { LoyaltyApi } from '../services/api'

export function useLoyaltyPoints() {
    const [points, setPoints] = useState<number | null>(null)
    const [loading, setLoading] = useState(true)

    const loadPoints = async () => {
        try {
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
    }, [])

    return { points, loading, refresh: loadPoints }
}