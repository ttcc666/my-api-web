import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { createAsyncCacheLoader } from '@/utils/asyncCacheLoader'

describe('createAsyncCacheLoader', () => {
  it('deduplicates concurrent loads and respects ttl', async () => {
    const lastLoadedAt = ref<number | null>(null)
    const loading = ref(false)
    const onSuccess = vi.fn()
    const fetcher = vi.fn().mockResolvedValue('data')

    const loader = createAsyncCacheLoader({
      ttl: 1_000,
      lastLoadedAt,
      loading,
      fetcher,
      onSuccess,
    })

    await Promise.all([loader.load(), loader.load()])

    expect(fetcher).toHaveBeenCalledTimes(1)
    expect(onSuccess).toHaveBeenCalledWith('data')
    expect(loader.isCacheValid()).toBe(true)
    expect(loading.value).toBe(false)

    // Cached load should not trigger fetcher again
    await loader.load()
    expect(fetcher).toHaveBeenCalledTimes(1)
  })

  it('supports explicit refresh when requested', async () => {
    const lastLoadedAt = ref<number | null>(null)
    const fetcher = vi.fn().mockResolvedValueOnce('first').mockResolvedValueOnce('second')
    const results: string[] = []

    const loader = createAsyncCacheLoader<string>({
      ttl: 10_000,
      lastLoadedAt,
      fetcher,
      onSuccess: (value) => {
        results.push(value)
      },
    })

    await loader.load()
    expect(fetcher).toHaveBeenCalledTimes(1)
    expect(results).toEqual(['first'])

    await loader.load(true)
    expect(fetcher).toHaveBeenCalledTimes(2)
    expect(results).toEqual(['first', 'second'])
  })
})
