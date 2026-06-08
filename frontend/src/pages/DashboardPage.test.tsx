import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { DashboardPage } from './DashboardPage'
import { AuthContext } from '../context/AuthContext'
import * as storesApi from '../api/stores'
import type { Store } from '../types'

vi.mock('../api/stores', () => ({
  getStores: vi.fn(),
  deleteStore: vi.fn(),
}))

const mockLogout = vi.fn()

function makeStore(overrides: Partial<Store> = {}): Store {
  return {
    id: '1',
    name: 'Test Store',
    availability: 'Standard',
    walletAddress: '0x1234567890abcdef',
    databaseType: 'Standard',
    status: 'Running',
    url: 'http://test-store.local',
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-01-01T00:00:00Z',
    ...overrides,
  }
}

function renderDashboard() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AuthContext.Provider value={{ token: 'tok', register: vi.fn(), login: vi.fn(), logout: mockLogout }}>
          <DashboardPage />
        </AuthContext.Provider>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
    vi.mocked(storesApi.deleteStore).mockResolvedValue(undefined)
    mockLogout.mockReset()
  })

  it('shows loading skeleton while fetching', () => {
    vi.mocked(storesApi.getStores).mockReturnValue(new Promise(() => {}))
    renderDashboard()
    expect(document.querySelectorAll('.animate-pulse')).toHaveLength(3)
  })

  it('shows empty state when no stores', async () => {
    vi.mocked(storesApi.getStores).mockResolvedValue([])
    renderDashboard()
    await waitFor(() => expect(screen.getByText('Nema prodavnica')).toBeInTheDocument())
  })

  it('renders store cards with name and status badge', async () => {
    vi.mocked(storesApi.getStores).mockResolvedValue([
      makeStore({ name: 'Moja Prodavnica', status: 'Running' }),
      makeStore({ id: '2', name: 'Druga Prodavnica', status: 'Pending' }),
    ])
    renderDashboard()
    await waitFor(() => expect(screen.getByText('Moja Prodavnica')).toBeInTheDocument())
    expect(screen.getByText('Druga Prodavnica')).toBeInTheDocument()
    expect(screen.getByText('Pokrenuta')).toBeInTheDocument()
    expect(screen.getByText('Kreiranje')).toBeInTheDocument()
  })

  it('Otvori button is disabled when store is not Running', async () => {
    vi.mocked(storesApi.getStores).mockResolvedValue([makeStore({ status: 'Pending', url: null })])
    renderDashboard()
    await waitFor(() => expect(screen.getByRole('button', { name: 'Otvori' })).toBeDisabled())
  })

  it('Otvori button is enabled when store is Running with url', async () => {
    vi.mocked(storesApi.getStores).mockResolvedValue([makeStore({ status: 'Running', url: 'http://shop.local' })])
    renderDashboard()
    await waitFor(() => expect(screen.getByRole('button', { name: 'Otvori' })).not.toBeDisabled())
  })

  it('calls deleteStore after confirm', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    vi.mocked(storesApi.getStores).mockResolvedValue([makeStore({ id: 'abc', name: 'Shop' })])
    renderDashboard()
    await waitFor(() => screen.getByRole('button', { name: 'Obriši' }))
    await userEvent.click(screen.getByRole('button', { name: 'Obriši' }))
    expect(vi.mocked(storesApi.deleteStore).mock.calls[0][0]).toBe('abc')
  })

  it('does not call deleteStore when confirm is cancelled', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false)
    vi.mocked(storesApi.getStores).mockResolvedValue([makeStore()])
    renderDashboard()
    await waitFor(() => screen.getByRole('button', { name: 'Obriši' }))
    await userEvent.click(screen.getByRole('button', { name: 'Obriši' }))
    expect(storesApi.deleteStore).not.toHaveBeenCalled()
  })

  it('calls logout when Odjavi se is clicked', async () => {
    vi.mocked(storesApi.getStores).mockResolvedValue([])
    renderDashboard()
    await waitFor(() => screen.getByRole('button', { name: 'Odjavi se' }))
    await userEvent.click(screen.getByRole('button', { name: 'Odjavi se' }))
    expect(mockLogout).toHaveBeenCalled()
  })

  it('shows error message on fetch failure', async () => {
    vi.mocked(storesApi.getStores).mockRejectedValue(new Error('Network error'))
    renderDashboard()
    await waitFor(() =>
      expect(screen.getByText(/Greška pri učitavanju/)).toBeInTheDocument(),
    )
  })
})
