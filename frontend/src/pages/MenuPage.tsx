import { useEffect, useState } from 'react'
import { MenuApi } from '../services/api'
import type { MenuItem } from '../types'

type Props = {
    onAddToCart: (item: MenuItem) => void
}

export default function MenuPage({ onAddToCart }: Props) {
    const [items, setItems] = useState<MenuItem[]>([])
    const [loading, setLoading] = useState(false)

    useEffect(() => {
        const load = async () => {
            setLoading(true)
            try {
                const data = await MenuApi.list()
                setItems(data)
            } finally {
                setLoading(false)
            }
        }
        load()
    }, [])

    if (loading) return <p>Loading...</p>

    return (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: 16 }}>
            {items.map(item => (
                <div key={item.id} style={{ border: '1px solid #ddd', padding: 16, borderRadius: 8 }}>
                    <h3>{item.name}</h3>
                    <p>{item.description || 'No description'}</p>
                    <p style={{ fontWeight: 'bold' }}>${item.price.toFixed(2)}</p>
                    <button onClick={() => onAddToCart(item)}>Add to Cart</button>
                </div>
            ))}
        </div>
    )
}
