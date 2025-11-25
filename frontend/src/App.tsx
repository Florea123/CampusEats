import { useState } from 'react'
import MenuPage from './pages/MenuPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import OrderCart from './components/OrderCart'
import { AuthApi } from './services/api'
import type { MenuItem } from './types'
import PaymentResult from './components/PaymentResult'
import { useLoyaltyPoints } from './hooks/useLoyaltyPoints'

type CartItem = { item: MenuItem; quantity: number }

export default function App() {
    const [view, setView] = useState<'login' | 'register' | 'app'>(
        AuthApi.getToken() ? 'app' : 'login'
    )
    const [cart, setCart] = useState<CartItem[]>([])
    const { points, loading: loadingPoints, refresh: refreshPoints } = useLoyaltyPoints()

    const onAuthDone = () => {
        setView('app')
        refreshPoints()
    }

    const addToCart = (item: MenuItem) => {
        setCart(prev => {
            const existing = prev.find(c => c.item.id === item.id)
            if (existing) {
                return prev.map(c =>
                    c.item.id === item.id ? { ...c, quantity: c.quantity + 1 } : c
                )
            }
            return [...prev, { item, quantity: 1 }]
        })
    }

    const updateQuantity = (itemId: string, qty: number) => {
        if (qty <= 0) {
            setCart(prev => prev.filter(c => c.item.id !== itemId))
        } else {
            setCart(prev => prev.map(c =>
                c.item.id === itemId ? { ...c, quantity: qty } : c
            ))
        }
    }

    const onPaymentSuccess = () => {
        setCart([])
        refreshPoints()
    }

    return (
        <div style={{ padding: 24, fontFamily: 'system-ui, sans-serif' }}>
            <header style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <h1 style={{ marginRight: 'auto' }}>CampusEats</h1>
                {view === 'app' && (
                    <div style={{
                        padding: '8px 16px',
                        backgroundColor: '#f0f0f0',
                        borderRadius: 20,
                        fontWeight: 600,
                        display: 'flex',
                        alignItems: 'center',
                        gap: 8
                    }}>
                        🎁 {loadingPoints ? '...' : points ?? 0} points
                    </div>
                )}
                {view === 'app' ? (
                    <button onClick={async () => { await AuthApi.logout(); setView('login'); setCart([]) }}>
                        Logout
                    </button>
                ) : (
                    <>
                        <button onClick={() => setView('login')} disabled={view === 'login'}>Login</button>
                        <button onClick={() => setView('register')} disabled={view === 'register'}>Register</button>
                    </>
                )}
            </header>

            <main style={{ marginTop: 24 }}>
                {view === 'app' && (
                    <>
                        <h2>Menu</h2>
                        <MenuPage onAddToCart={addToCart} />
                        <OrderCart cart={cart} onClear={() => setCart([])} onUpdateQuantity={updateQuantity} />
                    </>
                )}
                {view === 'login' && <LoginPage onLoggedIn={onAuthDone} />}
                {view === 'register' && <RegisterPage onRegistered={onAuthDone} />}
            </main>

            <PaymentResult onSuccess={onPaymentSuccess} />
        </div>
    )
}