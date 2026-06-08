import { createContext, useContext, useState, type ReactNode } from 'react'
import { login as loginApi, register as registerApi } from '../api/auth'
import { clearStoredToken, getStoredToken, setStoredToken } from '../api/client'

interface AuthContextValue {
  token: string | null
  register: (email: string, password: string) => Promise<void>
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(getStoredToken)

  async function register(email: string, password: string) {
    const res = await registerApi(email, password)
    setStoredToken(res.accessToken)
    setToken(res.accessToken)
  }

  async function login(email: string, password: string) {
    const res = await loginApi(email, password)
    setStoredToken(res.accessToken)
    setToken(res.accessToken)
  }

  function logout() {
    clearStoredToken()
    setToken(null)
  }

  return (
    <AuthContext.Provider value={{ token, register, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
