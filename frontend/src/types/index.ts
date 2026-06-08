export type StoreAvailability = 'Standard' | 'High'
export type DatabaseType = 'Standard' | 'Light'
export type StoreStatus = 'Pending' | 'Running' | 'Failed' | 'Deleting'

export interface Store {
  id: string
  name: string
  availability: StoreAvailability
  walletAddress: string
  databaseType: DatabaseType
  status: StoreStatus
  url: string | null
  createdAt: string
  updatedAt: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiry: string
}
