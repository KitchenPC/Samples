import { FormEvent, useEffect, useMemo, useState } from 'react'
import { aggregateShoppingList, getRecipe, searchRecipes } from './api'
import { loadShoppingList, saveShoppingList } from './storage'
import type { RecipeDetail, RecipeSummary, SavedShoppingList, ShoppingListResult } from './types'

function recipeIdFromHash() {
  const match = window.location.hash.match(/^#\/recipes\/([0-9a-f-]+)$/i)
  return match?.[1] ?? null
}

function minutesLabel(prep?: number, cook?: number) {
  const total = (prep ?? 0) + (cook ?? 0)
  return total > 0 ? `${total} min` : 'Flexible timing'
}

function RecipeImage({ recipe, large = false }: { recipe: RecipeSummary; large?: boolean }) {
  const [failed, setFailed] = useState(false)
  if (!recipe.imageUrl || failed) {
    return (
      <div className={`recipe-image placeholder ${large ? 'large' : ''}`} aria-hidden="true">
        <span>{recipe.title.slice(0, 1)}</span>
      </div>
    )
  }
  return (
    <img
      className={`recipe-image ${large ? 'large' : ''}`}
      src={recipe.imageUrl}
      alt=""
      onError={() => setFailed(true)}
    />
  )
}

function Header({ count, onOpenList }: { count: number; onOpenList: () => void }) {
  return (
    <header className="site-header">
      <a className="brand" href="#/" aria-label="KitchenPC Pantry home">
        <span className="brand-mark">K</span>
        <span>
          <strong>KitchenPC</strong>
          <small>Pantry</small>
        </span>
      </a>
      <button className="shopping-button" type="button" onClick={onOpenList}>
        <span aria-hidden="true">☷</span> Shopping list
        {count > 0 && <span className="count-badge">{count}</span>}
      </button>
    </header>
  )
}

function RecipeCard({ recipe, onOpen }: { recipe: RecipeSummary; onOpen: (id: string) => void }) {
  return (
    <article className="recipe-card">
      <button className="card-link" type="button" onClick={() => onOpen(recipe.id)}>
        <RecipeImage recipe={recipe} />
        <span className="card-body">
          <span className="eyebrow">{minutesLabel(recipe.prepTime, recipe.cookTime)}</span>
          <strong>{recipe.title}</strong>
          <span className="card-description">
            {recipe.description || 'A KitchenPC sample recipe ready to explore.'}
          </span>
          <span className="card-meta">
            {recipe.author ? `By ${recipe.author}` : 'KitchenPC community'}
            {recipe.averageRating > 0 && ` · ★ ${recipe.averageRating}`}
          </span>
        </span>
      </button>
    </article>
  )
}

function RecipeBrowser({ onOpen }: { onOpen: (id: string) => void }) {
  const [query, setQuery] = useState('')
  const [submittedQuery, setSubmittedQuery] = useState('')
  const [recipes, setRecipes] = useState<RecipeSummary[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    setError('')
    searchRecipes(submittedQuery, controller.signal)
      .then((result) => {
        setRecipes(result.recipes)
        setTotalCount(result.totalCount)
      })
      .catch((reason: Error) => {
        if (reason.name !== 'AbortError') setError(reason.message)
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })
    return () => controller.abort()
  }, [submittedQuery])

  function submit(event: FormEvent) {
    event.preventDefault()
    setSubmittedQuery(query.trim())
  }

  return (
    <main>
      <section className="hero">
        <div className="hero-copy">
          <span className="eyebrow">Cook from a smarter pantry</span>
          <h1>Find something worth making.</h1>
          <p>
            Browse sample recipes, see the details, and let KitchenPC turn ingredients into one
            tidy shopping list.
          </p>
          <form className="search-form" onSubmit={submit} role="search">
            <label className="sr-only" htmlFor="recipe-search">Search recipes</label>
            <input
              id="recipe-search"
              type="search"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Try brownies, chicken, or pasta"
            />
            <button type="submit">Search</button>
          </form>
        </div>
        <div className="hero-art" aria-hidden="true">
          <span className="plate">KPC</span>
          <span className="leaf leaf-one" />
          <span className="leaf leaf-two" />
        </div>
      </section>

      <section className="recipes-section" aria-live="polite">
        <div className="section-heading">
          <div>
            <span className="eyebrow">The sample collection</span>
            <h2>{submittedQuery ? `Results for “${submittedQuery}”` : 'Browse recipes'}</h2>
          </div>
          {!loading && !error && <span>{totalCount} recipe{totalCount === 1 ? '' : 's'}</span>}
        </div>
        {loading && <div className="status-panel">Gathering recipes…</div>}
        {error && <div className="status-panel error">{error}</div>}
        {!loading && !error && recipes.length === 0 && (
          <div className="status-panel">No recipes matched. Try a broader search.</div>
        )}
        <div className="recipe-grid">
          {recipes.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} onOpen={onOpen} />)}
        </div>
      </section>
    </main>
  )
}

function RecipePage({
  id,
  inList,
  onBack,
  onAdd,
  onOpenList,
}: {
  id: string
  inList: boolean
  onBack: () => void
  onAdd: (recipe: RecipeDetail) => void
  onOpenList: () => void
}) {
  const [recipe, setRecipe] = useState<RecipeDetail | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()
    setRecipe(null)
    setError('')
    getRecipe(id, controller.signal).then(setRecipe).catch((reason: Error) => {
      if (reason.name !== 'AbortError') setError(reason.message)
    })
    return () => controller.abort()
  }, [id])

  if (error) return <main className="detail-status"><p>{error}</p><button onClick={onBack}>Back to recipes</button></main>
  if (!recipe) return <main className="detail-status">Loading recipe…</main>

  return (
    <main className="recipe-detail">
      <button className="back-button" type="button" onClick={onBack}>← All recipes</button>
      <section className="detail-hero">
        <RecipeImage recipe={recipe} large />
        <div className="detail-copy">
          <span className="eyebrow">{recipe.tags.join(' · ') || 'KitchenPC recipe'}</span>
          <h1>{recipe.title}</h1>
          <p>{recipe.description}</p>
          <div className="detail-facts">
            <span><strong>{recipe.prepTime || '—'}</strong> prep min</span>
            <span><strong>{recipe.cookTime || '—'}</strong> cook min</span>
            <span><strong>{recipe.servingSize}</strong> servings</span>
          </div>
          <button
            className="primary-action"
            type="button"
            onClick={() => (inList ? onOpenList() : onAdd(recipe))}
          >
            {inList ? 'View in shopping list' : 'Add to shopping list'}
          </button>
          {(recipe.credit || recipe.author) && (
            <small className="credit">
              Recipe by {recipe.creditUrl ? <a href={recipe.creditUrl}>{recipe.credit}</a> : recipe.credit || recipe.author}
            </small>
          )}
        </div>
      </section>
      <section className="recipe-content">
        <div>
          <span className="eyebrow">What you’ll need</span>
          <h2>Ingredients</h2>
          <ul className="ingredient-list">
            {recipe.ingredients.map((ingredient, index) => <li key={`${ingredient.text}-${index}`}>{ingredient.text}</li>)}
          </ul>
        </div>
        <div>
          <span className="eyebrow">Step by step</span>
          <h2>Method</h2>
          <div className="method">{recipe.method || 'No preparation method was included with this sample recipe.'}</div>
        </div>
      </section>
    </main>
  )
}

function ShoppingDrawer({
  open,
  saved,
  result,
  loading,
  onClose,
  onAddRaw,
  onRemoveRaw,
  onRemoveRecipe,
  onToggle,
  onClear,
}: {
  open: boolean
  saved: SavedShoppingList
  result: ShoppingListResult
  loading: boolean
  onClose: () => void
  onAddRaw: (value: string) => void
  onRemoveRaw: (index: number) => void
  onRemoveRecipe: (id: string) => void
  onToggle: (key: string) => void
  onClear: () => void
}) {
  const [raw, setRaw] = useState('')
  function submit(event: FormEvent) {
    event.preventDefault()
    if (!raw.trim()) return
    onAddRaw(raw.trim())
    setRaw('')
  }

  return (
    <>
      <button className={`drawer-scrim ${open ? 'open' : ''}`} aria-label="Close shopping list" onClick={onClose} />
      <aside className={`shopping-drawer ${open ? 'open' : ''}`} aria-hidden={!open} aria-label="Shopping list">
        <div className="drawer-header">
          <div><span className="eyebrow">KitchenPC powered</span><h2>Shopping list</h2></div>
          <button className="icon-button" type="button" onClick={onClose} aria-label="Close">×</button>
        </div>
        <form className="quick-add" onSubmit={submit}>
          <label htmlFor="quick-item">Add an item in plain English</label>
          <div><input id="quick-item" value={raw} onChange={(event) => setRaw(event.target.value)} placeholder="e.g. 12 eggs" /><button>Add</button></div>
        </form>
        {saved.recipeIds.length > 0 && (
          <section className="list-sources">
            <h3>Recipes</h3>
            {saved.recipeIds.map((id) => (
              <div key={id}><span>{saved.recipeTitles[id] || 'Recipe'}</span><button onClick={() => onRemoveRecipe(id)}>Remove</button></div>
            ))}
          </section>
        )}
        {saved.items.length > 0 && (
          <section className="list-sources">
            <h3>Quick additions</h3>
            {saved.items.map((item, index) => (
              <div key={`${item}-${index}`}><span>{item}</span><button onClick={() => onRemoveRaw(index)}>Remove</button></div>
            ))}
          </section>
        )}
        <section className="aggregated-list" aria-live="polite">
          <h3>Combined list</h3>
          {loading && <p>Combining ingredients…</p>}
          {!loading && result.items.length === 0 && result.unrecognizedItems.length === 0 && <p>Your list is ready for something delicious.</p>}
          {result.items.map((item) => {
            const checked = saved.checkedKeys.includes(item.key)
            return (
              <label className={checked ? 'checked' : ''} key={item.key}>
                <input type="checkbox" checked={checked} onChange={() => onToggle(item.key)} />
                <span><strong>{item.name}</strong>{item.amount && <small>{item.amount}</small>}</span>
              </label>
            )
          })}
          {result.unrecognizedItems.map((item, index) => (
            <div className="unrecognized" key={`${item}-${index}`}><strong>{item}</strong><small>Kept as written</small></div>
          ))}
        </section>
        {(saved.recipeIds.length > 0 || saved.items.length > 0) && <button className="clear-button" onClick={onClear}>Clear shopping list</button>}
      </aside>
    </>
  )
}

export default function App() {
  const [selectedRecipeId, setSelectedRecipeId] = useState(recipeIdFromHash)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [saved, setSaved] = useState<SavedShoppingList>(loadShoppingList)
  const [result, setResult] = useState<ShoppingListResult>({ items: [], unrecognizedItems: [] })
  const [listLoading, setListLoading] = useState(false)

  useEffect(() => {
    const updateRoute = () => setSelectedRecipeId(recipeIdFromHash())
    window.addEventListener('hashchange', updateRoute)
    return () => window.removeEventListener('hashchange', updateRoute)
  }, [])

  useEffect(() => saveShoppingList(saved), [saved])

  useEffect(() => {
    const controller = new AbortController()
    if (saved.recipeIds.length === 0 && saved.items.length === 0) {
      setResult({ items: [], unrecognizedItems: [] })
      return () => controller.abort()
    }
    setListLoading(true)
    aggregateShoppingList(saved.recipeIds, saved.items, controller.signal)
      .then(setResult)
      .catch((reason: Error) => {
        if (reason.name !== 'AbortError') setResult({ items: [], unrecognizedItems: ['Unable to refresh the list.'] })
      })
      .finally(() => {
        if (!controller.signal.aborted) setListLoading(false)
      })
    return () => controller.abort()
  }, [saved.recipeIds, saved.items])

  const itemCount = useMemo(() => result.items.length + result.unrecognizedItems.length, [result])

  function openRecipe(id: string) { window.location.hash = `/recipes/${id}` }
  function backToRecipes() { window.location.hash = '/' }
  function addRecipe(recipe: RecipeDetail) {
    setSaved((current) => current.recipeIds.includes(recipe.id) ? current : {
      ...current,
      recipeIds: [...current.recipeIds, recipe.id],
      recipeTitles: { ...current.recipeTitles, [recipe.id]: recipe.title },
    })
    setDrawerOpen(true)
  }

  return (
    <div className="app-shell">
      <Header count={itemCount} onOpenList={() => setDrawerOpen(true)} />
      {selectedRecipeId ? (
        <RecipePage
          id={selectedRecipeId}
          inList={saved.recipeIds.includes(selectedRecipeId)}
          onBack={backToRecipes}
          onAdd={addRecipe}
          onOpenList={() => setDrawerOpen(true)}
        />
      ) : <RecipeBrowser onOpen={openRecipe} />}
      <ShoppingDrawer
        open={drawerOpen}
        saved={saved}
        result={result}
        loading={listLoading}
        onClose={() => setDrawerOpen(false)}
        onAddRaw={(value) => setSaved((current) => ({ ...current, items: [...current.items, value] }))}
        onRemoveRaw={(index) => setSaved((current) => ({ ...current, items: current.items.filter((_, itemIndex) => itemIndex !== index) }))}
        onRemoveRecipe={(id) => setSaved((current) => ({ ...current, recipeIds: current.recipeIds.filter((recipeId) => recipeId !== id) }))}
        onToggle={(key) => setSaved((current) => ({ ...current, checkedKeys: current.checkedKeys.includes(key) ? current.checkedKeys.filter((value) => value !== key) : [...current.checkedKeys, key] }))}
        onClear={() => setSaved({ recipeIds: [], recipeTitles: {}, items: [], checkedKeys: [] })}
      />
      <footer><span>KitchenPC Pantry sample</span><span>React + ASP.NET Core + PostgreSQL</span></footer>
    </div>
  )
}
