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
  availability: StoreAvailability
  walletAddress: string
  databaseType: DatabaseType
}

export async function createStore(payload: CreateStorePayload): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>('/stores', payload)
  return data
}

export interface UpdateStorePayload {
  availability: StoreAvailability
  walletAddress: string
}

export async function updateStore(id: string, payload: UpdateStorePayload): Promise<void> {
  await client.put(`/stores/${id}`, payload)
}

export async function deleteStore(id: string): Promise<void> {
  await client.delete(`/stores/${id}`)
}

export const AvailabilityOptions: { label: string; value: StoreAvailability }[] = [
  { label: 'Standard (2 replike)', value: 'Standard' },
  { label: 'High (3 replike)', value: 'High' },
]

export const DatabaseTypeOptions: { label: string; value: DatabaseType }[] = [
  { label: 'Standard (PostgreSQL)', value: 'Standard' },
  { label: 'Light (Redis)', value: 'Light' },
]
