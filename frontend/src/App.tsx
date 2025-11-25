import { useState, useEffect } from 'react'
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom'
import { jwtDecode } from 'jwt-decode'
import { AuthApi } from './services/api'
import type { MenuItem } from './types'

// Pagini
import MenuPage from './pages/MenuPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import OrdersPage from './pages/OrdersPage'
import KitchenDashboard from './pages/KitchenDashboard'
import MenuForm from './components/MenuForm' // Folosim componenta existentă ca pagină de admin

// Componente
import OrderCart from './components/OrderCart'
import PaymentResult from './components/PaymentResult'
import { useLoyaltyPoints } from './hooks/useLoyaltyPoints'

type CartItem = { item: MenuItem; quantity: number }

// Wrapper pentru Layout care primește props-urile necesare
function Layout({ children, role, onLogout, cartCount }: any) {
    const { points } = useLoyaltyPoints()

    return (
        <div style={{ fontFamily: 'system-ui, sans-serif', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
            <header style={{ 
                padding: '16px 24px', 
                borderBottom: '1px solid #eee', 
                display: 'flex', 
                alignItems: 'center', 
                gap: 20, 
                backgroundColor: '#fff',
                position: 'sticky',
                top: 0,
                zIndex: 100
            }}>
                <h1 style={{ margin: 0, marginRight: 'auto', fontSize: '1.5rem' }}>
                    <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>CampusEats 🍕</Link>
                </h1>
                
                <nav style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
                    <Link to="/" style={{ textDecoration: 'none', color: '#333' }}>Meniu</Link>
                    
                    {role && <Link to="/orders" style={{ textDecoration: 'none', color: '#333' }}>Comenzi</Link>}
                    
                    {(role === 'WORKER' || role === 'MANAGER') && (
                        <Link to="/kitchen" style={{ color: '#d32f2f', fontWeight: 'bold', textDecoration: 'none' }}>Bucătărie</Link>
                    )}
                    
                    {(role === 'MANAGER') && (
                        <Link to="/admin/menu" style={{ textDecoration: 'none', color: '#333' }}>Admin Meniu</Link>
                    )}
                </nav>

                {role && (
                    <div style={{ background: '#f0f0f0', padding: '6px 12px', borderRadius: 20, fontSize: '0.9rem' }}>
                        🎁 {points ?? 0} pct
                    </div>
                )}
                
                {role ? (
                    <button onClick={onLogout} style={{ padding: '6px 12px', cursor: 'pointer' }}>Logout</button>
                ) : (
                    <div style={{ display: 'flex', gap: 8 }}>
                        <Link to="/login"><button style={{ cursor: 'pointer' }}>Login</button></Link>
                        <Link to="/register"><button style={{ cursor: 'pointer' }}>Register</button></Link>
                    </div>
                )}
            </header>
            
            <main style={{ flex: 1, backgroundColor: '#fafafa' }}>
                {children}
            </main>
        </div>
    )
}

export default function App() {
    const [token, setToken] = useState<string | null>(AuthApi.getToken())
    const [role, setRole] = useState<string | null>(null)
    const [cart, setCart] = useState<CartItem[]>([])

    useEffect(() => {
        if (token) {
            try {
                const decoded: any = jwtDecode(token)
                // Backend-ul .NET pune rolul în cheia specifică sau 'role'
                const userRole = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded.role;
                setRole(userRole)
            } catch (e) {
                console.error("Invalid token", e)
                setRole(null)
            }
        } else {
            setRole(null)
        }
    }, [token])

    const handleLogout = async () => {
        await AuthApi.logout()
        setToken(null)
        setCart([])
        window.location.href = '/'
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
        alert("Plată realizată cu succes! Punctele au fost adăugate.")
        // Refresh points logic handled by hook inside Layout re-render or explicit reload
        window.location.href = '/orders'
    }

    return (
        <BrowserRouter>
            <Layout role={role} onLogout={handleLogout} cartCount={cart.length}>
                <Routes>
                    <Route path="/" element={
                        <>
                            <div style={{ padding: 24 }}>
                                <MenuPage onAddToCart={addToCart} />
                            </div>
                            {/* Coșul e vizibil doar dacă ești autentificat */}
                            {token && (
                                <OrderCart 
                                    cart={cart} 
                                    onClear={() => setCart([])} 
                                    onUpdateQuantity={updateQuantity} 
                                />
                            )}
                        </>
                    } />
                    
                    <Route path="/login" element={
                        !token ? <div style={{ display:'flex', justifyContent:'center', marginTop: 40 }}><LoginPage onLoggedIn={() => setToken(AuthApi.getToken())} /></div> : <Navigate to="/" />
                    } />
                    
                    <Route path="/register" element={
                        !token ? <div style={{ display:'flex', justifyContent:'center', marginTop: 40 }}><RegisterPage onRegistered={() => setToken(AuthApi.getToken())} /></div> : <Navigate to="/" />
                    } />

                    <Route path="/orders" element={
                        token ? <OrdersPage /> : <Navigate to="/login" />
                    } />
                    
                    <Route path="/kitchen" element={
                        (role === 'WORKER' || role === 'MANAGER') ? <KitchenDashboard /> : <Navigate to="/" />
                    } />
                    
                    <Route path="/admin/menu" element={
                        (role === 'MANAGER') ? <div style={{ display:'flex', justifyContent:'center', marginTop: 40 }}><MenuForm /></div> : <Navigate to="/" />
                    } />
                </Routes>
                
                <PaymentResult onSuccess={onPaymentSuccess} />
            </Layout>
        </BrowserRouter>
    )
}