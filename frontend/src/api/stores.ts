import type { Store, StoreAvailability, DatabaseType } from '../types'
import client from './client'

export async function getStores(): Promise<Store[]> {
  const { data } = await client.get<Store[]>('/stores')
  return data
}

export async function getStore(id: string): Promise<Store> {
  const { data } = await client.get<Store>(`/stores/${id}`)
  return data
}

export interface CreateStorePayload {
  name: string
  availability: number
  walletAddress: string
  databaseType: number
}

export async function createStore(payload: CreateStorePayload): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>('/stores', payload)
  return data
}

export interface UpdateStorePayload {
  availability: number
  walletAddress: string
}

export async function updateStore(id: string, payload: UpdateStorePayload): Promise<void> {
  await client.put(`/stores/${id}`, payload)
}

export async function deleteStore(id: string): Promise<void> {
  await client.delete(`/stores/${id}`)
}

export const AvailabilityOptions: { label: string; value: number; key: StoreAvailability }[] = [
  { label: 'Standard (2 replike)', value: 2, key: 'Standard' },
  { label: 'High (3 replike)', value: 3, key: 'High' },
]

export const DatabaseTypeOptions: { label: string; value: number; key: DatabaseType }[] = [
  { label: 'Standard (PostgreSQL)', value: 0, key: 'Standard' },
  { label: 'Light (Redis)', value: 1, key: 'Light' },
]
