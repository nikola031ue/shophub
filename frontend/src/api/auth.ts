import type { TokenResponse } from '../types'
import client from './client'

export async function register(email: string, password: string): Promise<TokenResponse> {
  const { data } = await client.post<TokenResponse>('/auth/register', { email, password })
  return data
}

export async function login(email: string, password: string): Promise<TokenResponse> {
  const { data } = await client.post<TokenResponse>('/auth/login', { email, password })
  return data
}

export async function refresh(accessToken: string, refreshToken: string): Promise<TokenResponse> {
  const { data } = await client.post<TokenResponse>('/auth/refresh', { accessToken, refreshToken })
  return data
}
