
import { useState, useRef, useEffect } from 'react'
import './SearchBar.css'

function SearchBar({
  search,
  setSearch,
  onSearch,
  products = [],
  onProductClick
}) {
  const [photo, setPhoto] = useState(null)
  const [cameraOpen, setCameraOpen] = useState(false)
  const [showSuggestions, setShowSuggestions] = useState(false)

const filteredSuggestions = search.trim().length > 0
  ? products.filter(product =>
      product.name?.toLowerCase().includes(search.trim().toLowerCase())
    ).slice(0, 6)
  : []

  const videoRef = useRef(null)
  const streamRef = useRef(null)

  // SEARCH
  const handleSearch = (e) => {
    const value = e.target.value
    setSearch(value)
    onSearch?.()
  }

  // VOICE SEARCH
  const startVoiceSearch = () => {
    const SpeechRecognition =
      window.SpeechRecognition || window.webkitSpeechRecognition

    if (!SpeechRecognition) {
      alert('Voice search is not supported in this browser.')
      return
    }

    const recognition = new SpeechRecognition()
    recognition.lang = 'en-IN'
    recognition.interimResults = false

    recognition.onresult = (event) => {
      const voiceText = event.results[0][0].transcript
      setSearch(voiceText)
      onSearch?.()
    }

    recognition.onerror = () => {
      alert('Voice search failed. Please try again.')
    }

    recognition.start()
  }

  // OPEN CAMERA
  const openCamera = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      })

      streamRef.current = stream
      setCameraOpen(true)
    } catch (error) {
      alert('Unable to access camera. Please check camera permission.')
    }
  }

  // CONNECT VIDEO
  useEffect(() => {
    if (cameraOpen && videoRef.current && streamRef.current) {
      videoRef.current.srcObject = streamRef.current
    }
  }, [cameraOpen])

  // CLOSE CAMERA
  const closeCamera = () => {
    streamRef.current?.getTracks().forEach(track => track.stop())
    streamRef.current = null
    setCameraOpen(false)
  }

  // TAKE PHOTO
  const takePhoto = () => {
    const video = videoRef.current

    if (!video || !video.videoWidth) return

    const canvas = document.createElement('canvas')
    canvas.width = video.videoWidth
    canvas.height = video.videoHeight

    const context = canvas.getContext('2d')
    context.drawImage(video, 0, 0)

    setPhoto(canvas.toDataURL('image/png'))
    closeCamera()
  }

  // STOP CAMERA WHEN COMPONENT UNMOUNTS
  useEffect(() => {
    return () => {
      streamRef.current?.getTracks().forEach(track => track.stop())
    }
  }, [])

  return (
    <div className="search-tools">

      <div className="search-box">

        <input
          type="text"
          placeholder="Search for products..."
          className="search"
          value={search}
          onChange={(e) => {
  handleSearch(e)
  setShowSuggestions(true)
}}
onFocus={() => {
  if (search.trim()) setShowSuggestions(true)
}}
onBlur={() => {
  setTimeout(() => setShowSuggestions(false), 200)
}}
        />

        {/* VOICE SEARCH */}
        <button
          className="search-icon-btn"
          onClick={startVoiceSearch}
          type="button"
          title="Voice Search"
          aria-label="Voice Search"
        >
          <svg viewBox="0 0 24 24">
            <rect x="9" y="2" width="6" height="12" rx="3" />
            <path d="M5 10a7 7 0 0 0 14 0M12 17v5m-4 0h8" />
          </svg>
        </button>

        {/* CAMERA */}
        <button
          className="search-icon-btn"
          onClick={openCamera}
          type="button"
          title="Search with Camera"
          aria-label="Open Camera"
        >
          <svg viewBox="0 0 24 24">
            <path d="M3 7h4l2-3h6l2 3h4v13H3z" />
            <circle cx="12" cy="13" r="4" />
          </svg>
        </button>

      </div>
      {showSuggestions && search.trim() && (
  <div className="search-suggestions">
    {filteredSuggestions.length > 0 ? (
      filteredSuggestions.map(product => (
   <div
  key={product.id}
  className="suggestion-item"
  onMouseDown={(e) => {
    e.preventDefault()
    console.log('SEARCH PRODUCT CLICKED:', product)
    setSearch(product.name)
    setShowSuggestions(false)
    onProductClick?.(product)
  }}
>
        <div className="suggestion-name">
  <strong>{product.name}</strong>
</div>
        </div>
      ))
    ) : (
      <p className="no-suggestions">
        No matching products found
      </p>
    )}
  </div>
)}

      {/* CAMERA MODAL */}
      {cameraOpen && (
        <div className="camera-overlay">
          <div className="camera-modal">

            <div className="camera-header">
              <span>Capture Product Image</span>

              <button
                className="camera-close"
                onClick={closeCamera}
                type="button"
              >
                ✕
              </button>
            </div>

            <video
              ref={videoRef}
              autoPlay
              playsInline
              className="camera-video"
            />

            <div className="camera-controls">
              <button
                className="shutter-button"
                onClick={takePhoto}
                type="button"
                title="Take Photo"
              >
                <span />
              </button>
            </div>

            <p className="camera-hint">
              Tap to capture your photo
            </p>

          </div>
        </div>
      )}

      {/* PHOTO PREVIEW */}
      {photo && (
        <div className="photo-preview">
          <img src={photo} alt="Captured product" />

          <button
            type="button"
            onClick={() => setPhoto(null)}
            aria-label="Remove photo"
          >
            ✕
          </button>
        </div>
      )}

    </div>
  )
}

export default SearchBar
