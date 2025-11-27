import { useEffect, useState } from 'react'
import { MenuApi } from '../services/api'
import type { MenuItem } from '../types'
import { Plus } from 'lucide-react'

type Props = {
    onAddToCart: (item: MenuItem) => void
    isLoggedIn?: boolean
}

export default function MenuPage({ onAddToCart, isLoggedIn }: Props) {
    const [items, setItems] = useState<MenuItem[]>([])
    const [loading, setLoading] = useState(true)

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

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-brand-600"></div>
        </div>
    )

    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {items.map(item => (
                <div key={item.id} className="group bg-white rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300 border border-gray-100 overflow-hidden flex flex-col hover:-translate-y-1">
                    {/* Imagine / Placeholder */}
                    <div className="relative h-48 bg-gray-100 overflow-hidden flex items-center justify-center">
                        {item.imageUrl ? (
                            <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                        ) : (
                            <span className="text-6xl transform group-hover:scale-110 transition-transform">🍽️</span>
                        )}
                        <div className="absolute top-3 right-3 bg-white/95 backdrop-blur-sm px-3 py-1 rounded-lg text-sm font-bold text-gray-900 shadow-sm border border-gray-100">
                            {item.price.toFixed(2)} RON
                        </div>
                    </div>
                    
                    <div className="p-5 flex flex-col flex-1">
                        <h3 className="text-lg font-bold text-gray-900 mb-1">{item.name}</h3>
                        <p className="text-gray-500 text-sm line-clamp-2 mb-4 flex-1 min-h-[2.5em]">
                            {item.description || 'Un preparat delicios pregătit proaspăt.'}
                        </p>
                        
                        {item.allergens && item.allergens.length > 0 && (
                            <div className="flex gap-1 flex-wrap mb-4">
                                {item.allergens.map(a => (
                                    <span key={a} className="text-[10px] uppercase font-bold tracking-wider bg-rose-50 text-rose-600 px-2 py-1 rounded border border-rose-100">
                                        {a}
                                    </span>
                                ))}
                            </div>
                        )}

                        {isLoggedIn && (
                            <button
                                onClick={() => onAddToCart(item)}
                                className="w-full flex items-center justify-center gap-2 bg-gray-900 hover:bg-brand-600 active:bg-brand-700 text-white py-3 rounded-xl font-bold text-sm transition-all shadow-lg shadow-gray-200 hover:shadow-brand-500/30"
                            >
                                <Plus size={18} />
                                Adaugă în coș
                            </button>
                        )}
                    </div>
                </div>
            ))}
        </div>
    )
}