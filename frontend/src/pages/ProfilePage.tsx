import { useEffect, useState } from "react";
import { AuthApi,UserDto } from "../services/api";

export default function ProfilePage() {
    const [user, setUser] = useState<UserDto | null>(null);
    const [loading, setLoading] = useState(true);
    const[formData,setFormData]=useState({
        name:'',
        addressCity:'',
        addressStreet:'',
        addressNumber:'',
        addressDetails:'',
        profilePictureUrl:''
    });
    useEffect(() => {
        const loadProfile = async () => {
            try{
                const data = await AuthApi.getMe();
                setUser(data);
                setFormData({
                    name:data.name || '',
                    addressCity:data.addressCity || '',
                    addressStreet:data.addressStreet || '',
                    addressNumber:data.addressNumber || '',
                    addressDetails:data.addressDetails || '',
                    profilePictureUrl:data.profilePictureUrl || ''
                });
            }
            catch(err){
                alert("Nu s-a putut încărca profilul: " + (err as Error).message);
            }
            finally{
                setLoading(false);
            }
        }
        loadProfile();
    }, []);

    if(loading){
        return <div className="p-10 text-center">Se încarcă profilul...</div>
    }
    const handleSubmit=async(e : React.FormEvent)=>{
        e.preventDefault();
        try{
            await AuthApi.updateProfile(formData);
            alert("Profil actualizat cu succes!");
        }
        catch(err){
            console.error(err);
            alert("Eroare la actualizarea profilului: " + (err as Error).message);
        }
    }

   return (
        <div className="p-4 max-w-2xl mx-auto">
            <h1 className="text-2xl font-bold mb-6">Profilul Meu</h1>

     
            <form onSubmit={handleSubmit} className="space-y-4">
                
               
                
                <div className="mb-4">
                    <label className="block font-bold mb-1">Email:</label>
                    <input 
                        type="email" 
                        value={user?.email || ''} 
                        disabled={true}
                        className="border p-2 rounded w-full bg-gray-100 text-gray-500 cursor-not-allowed"
                    />
                </div>

                <div className="mb-4">
                    <label className="block font-bold mb-1">Nume:</label>
                    <input 
                        type="text" 
                        className="border p-2 rounded w-full"
                        value={formData.name} 
                        onChange={e => setFormData({...formData, name: e.target.value})} 
                    />
                </div>

           
                <div className="mb-4">
                    <label className="block font-bold mb-1">URL Poză Profil:</label>
                    <input type="text" className="border p-2 rounded w-full" value={formData.profilePictureUrl} onChange={e => setFormData({...formData, profilePictureUrl: e.target.value})} />
                </div>
                <div className="mb-4">
                    <label className="block font-bold mb-1">Oraș:</label>
                    <input type="text" className="border p-2 rounded w-full" value={formData.addressCity} onChange={e => setFormData({...formData, addressCity: e.target.value})} />
                </div>
                <div className="mb-4">
                    <label className="block font-bold mb-1">Stradă:</label>
                    <input type="text" className="border p-2 rounded w-full" value={formData.addressStreet} onChange={e => setFormData({...formData, addressStreet: e.target.value})} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                    <div className="mb-4">
                        <label className="block font-bold mb-1">Număr:</label>
                        <input type="text" className="border p-2 rounded w-full" value={formData.addressNumber} onChange={e => setFormData({...formData, addressNumber: e.target.value})} />
                    </div>
                    <div className="mb-4">
                        <label className="block font-bold mb-1">Detalii:</label>
                        <input type="text" className="border p-2 rounded w-full" value={formData.addressDetails} onChange={e => setFormData({...formData, addressDetails: e.target.value})} />
                    </div>
                </div>

             
                <button 
                    type="submit" 
                    className="w-full bg-blue-600 text-white font-bold py-3 px-4 rounded hover:bg-blue-700 transition duration-200 mt-4"
                >
                    Salvează Modificările
                </button>

            </form>
        </div>
    )
}