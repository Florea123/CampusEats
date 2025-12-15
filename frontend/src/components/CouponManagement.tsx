import { useState, useEffect } from 'react'
import { CouponApi, MenuApi } from '../services/api'
import { CreateCouponRequest, CouponType, MenuItem, CouponDto } from '../types'

export function CouponManagement() {
    const [menuItems, setMenuItems] = useState<MenuItem[]>([])
    const [existingCoupons, setExistingCoupons] = useState<CouponDto[]>([])
    const [formData, setFormData] = useState<CreateCouponRequest>({
        name: '',
        description: '',
        type: CouponType.PercentageDiscount,
        discountValue: 0,
        pointsCost: 0,
        specificMenuItemId: null,
        minimumOrderAmount: null,
        expiresAtUtc: null
    })
    const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null)
    const [loading, setLoading] = useState(false)

    useEffect(() => {
        loadMenuItems()
        loadCoupons()
    }, [])

    const loadMenuItems = async () => {
        try {
            const items = await MenuApi.list()
            setMenuItems(items)
        } catch (err) {
            console.error('Failed to load menu items', err)
        }
    }

    const loadCoupons = async () => {
        try {
            const coupons = await CouponApi.getAvailable()
            setExistingCoupons(coupons)
        } catch (err) {
            console.error('Failed to load coupons', err)
        }
    }

    const handleDelete = async (couponId: string) => {
        if (!confirm('Ești sigur că vrei să ștergi acest cupon?')) {
            return
        }

        try {
            const result = await CouponApi.delete(couponId)
            if (result.success) {
                loadCoupons() // Reload the list
            } else {
                setMessage({ type: 'error', text: result.message })
                setTimeout(() => setMessage(null), 5000)
            }
        } catch (err: any) {
            setMessage({ type: 'error', text: err.message || 'Eroare la ștergerea cuponului' })
            setTimeout(() => setMessage(null), 5000)
        }
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setLoading(true)
        setMessage(null)

        try {
            const result = await CouponApi.create(formData)
            if (result.success) {
                setMessage({ type: 'success', text: 'Cupon creat cu succes!' })
                setFormData({
                    name: '',
                    description: '',
                    type: CouponType.PercentageDiscount,
                    discountValue: 0,
                    pointsCost: 0,
                    specificMenuItemId: null,
                    minimumOrderAmount: null,
                    expiresAtUtc: null
                })
                loadCoupons() // Reload the list
            } else {
                setMessage({ type: 'error', text: result.message })
            }
        } catch (err: any) {
            setMessage({ type: 'error', text: err.message || 'Eroare la crearea cuponului' })
        } finally {
            setLoading(false)
            setTimeout(() => setMessage(null), 5000)
        }
    }

    const getCouponTypeLabel = (type: CouponType) => {
        switch (type) {
            case CouponType.PercentageDiscount:
                return 'Reducere Procentuală'
            case CouponType.FixedAmountDiscount:
                return 'Reducere Fixă'
            case CouponType.FreeItem:
                return 'Produs Gratuit'
            default:
                return 'Unknown'
        }
    }

    return (
        <div className="space-y-6">
            {/* Form creare cupon nou */}
            <div className="bg-white rounded-lg shadow-md p-6">
                <h2 className="text-2xl font-bold mb-6">Creare Cupon Nou</h2>

                {message && (
                    <div className={`mb-4 p-4 rounded-lg ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {message.text}
                    </div>
                )}

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Nume Cupon</label>
                    <input
                        type="text"
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                        placeholder="ex: Weekend Special 20%"
                        required
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Descriere</label>
                    <textarea
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                        rows={3}
                        placeholder="ex: Primești 20% reducere la orice comandă în acest weekend!"
                        required
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Tip Cupon</label>
                    <select
                        value={formData.type}
                        onChange={(e) => {
                            const newType = Number(e.target.value) as CouponType
                            setFormData({ 
                                ...formData, 
                                type: newType,
                                discountValue: newType === CouponType.FreeItem ? 0 : formData.discountValue
                            })
                        }}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                    >
                        <option value={CouponType.PercentageDiscount}>Reducere Procentuală (%) - se aplică la toată comanda</option>
                        <option value={CouponType.FixedAmountDiscount}>Reducere Fixă (lei) - se scade din totalul comenzii</option>
                        <option value={CouponType.FreeItem}>Produs Gratuit - un produs devine gratuit</option>
                    </select>
                </div>

                {formData.type !== CouponType.FreeItem && (
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            {formData.type === CouponType.PercentageDiscount ? 'Procent Reducere (%)' : 'Valoare Reducere (lei)'}
                        </label>
                        <input
                            type="number"
                            value={formData.discountValue || ''}
                            onChange={(e) => setFormData({ ...formData, discountValue: e.target.value ? Number(e.target.value) : 0 })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                            min="0.01"
                            step="0.01"
                            placeholder={formData.type === CouponType.PercentageDiscount ? 'ex: 20' : 'ex: 10.00'}
                            required
                        />
                    </div>
                )}

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Cost în Puncte</label>
                    <input
                        type="number"
                        value={formData.pointsCost || ''}
                        onChange={(e) => setFormData({ ...formData, pointsCost: e.target.value ? Number(e.target.value) : 0 })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                        min="1"
                        placeholder="ex: 100"
                        required
                    />
                </div>

                {formData.type === CouponType.FreeItem && (
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Produs Specific (Opțional)</label>
                        <select
                            value={formData.specificMenuItemId || ''}
                            onChange={(e) => setFormData({ ...formData, specificMenuItemId: e.target.value || null })}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                        >
                            <option value="">-- Selectează Produs --</option>
                            {menuItems.map(item => (
                                <option key={item.id} value={item.id}>{item.name} - {item.price} lei</option>
                            ))}
                        </select>
                    </div>
                )}

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Valoare Minimă Comandă (lei) - Opțional</label>
                    <input
                        type="number"
                        value={formData.minimumOrderAmount || ''}
                        onChange={(e) => setFormData({ ...formData, minimumOrderAmount: e.target.value ? Number(e.target.value) : null })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                        min="0"
                        step="0.01"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Data Expirare (Opțional)</label>
                    <input
                        type="datetime-local"
                        value={formData.expiresAtUtc ? new Date(formData.expiresAtUtc).toISOString().slice(0, 16) : ''}
                        onChange={(e) => setFormData({ ...formData, expiresAtUtc: e.target.value ? new Date(e.target.value).toISOString() : null })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-brand-500"
                    />
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="w-full bg-brand-600 text-white py-3 rounded-lg font-semibold hover:bg-brand-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
                >
                    {loading ? 'Se creează...' : 'Creare Cupon'}
                </button>
            </form>
        </div>

            {/* Lista cupoane existente */}
            <div className="bg-white rounded-lg shadow-md p-6">
                <h2 className="text-2xl font-bold mb-4">Cupoane Existente</h2>
                {existingCoupons.length === 0 ? (
                    <p className="text-gray-500">Nu există cupoane create încă.</p>
                ) : (
                    <div className="space-y-3">
                        {existingCoupons.map(coupon => (
                            <div key={coupon.id} className="border border-gray-200 rounded-lg p-4 flex justify-between items-start">
                                <div className="flex-1">
                                    <h3 className="font-semibold text-lg">{coupon.name}</h3>
                                    <p className="text-gray-600 text-sm mt-1">{coupon.description}</p>
                                    <div className="flex gap-4 mt-2 text-sm">
                                        <span className="text-gray-700">
                                            <strong>Tip:</strong> {getCouponTypeLabel(coupon.type)}
                                        </span>
                                        {coupon.discountValue > 0 && (
                                            <span className="text-gray-700">
                                                <strong>Valoare:</strong> {coupon.type === CouponType.PercentageDiscount ? `${coupon.discountValue}%` : `${coupon.discountValue} lei`}
                                            </span>
                                        )}
                                        <span className="text-gray-700">
                                            <strong>Cost:</strong> {coupon.pointsCost} puncte
                                        </span>
                                        {coupon.minimumOrderAmount && (
                                            <span className="text-gray-700">
                                                <strong>Comandă minimă:</strong> {coupon.minimumOrderAmount} lei
                                            </span>
                                        )}
                                    </div>
                                    {coupon.expiresAtUtc && (
                                        <p className="text-sm text-gray-500 mt-1">
                                            Expiră: {new Date(coupon.expiresAtUtc).toLocaleDateString('ro-RO')}
                                        </p>
                                    )}
                                    <span className={`inline-block mt-2 px-2 py-1 rounded text-xs ${coupon.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                                        {coupon.isActive ? 'Activ' : 'Inactiv'}
                                    </span>
                                </div>
                                <button
                                    onClick={() => handleDelete(coupon.id)}
                                    className="ml-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                                >
                                    Șterge
                                </button>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    )
}
