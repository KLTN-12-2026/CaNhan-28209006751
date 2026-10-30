import {
  ExpandLess,
  ExpandMore,
  RefreshOutlined,
} from '@mui/icons-material';
import { ClickAwayListener, LinearProgress } from '@mui/material';
import { useEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { updateAuthenticatedUser } from '../auth/authSession';
import { getProfile } from '../profile/profileApi';
import { PageHeader } from '../layout/PageHeader';
import { sanitizeLessonHtml } from '../../utils/sanitizeHtml';
import {
  getAboutContent,
  getChapters,
  saveLessonProgress,
  AboutContent,
  HomeChapter,
  HomeLesson,
} from './homeApi';
import { LookupView } from './LookupView';
import './home.css';

const homeApiBaseUrl = (import.meta.env.VITE_API_URL ?? import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7001').replace(/\/$/, '');

const selectedChapterStorageKey = 'atomos-home-selected-chapter-id';

const readStoredChapterId = () => {
  const value = Number(localStorage.getItem(selectedChapterStorageKey));
  return Number.isFinite(value) && value > 0 ? value : null;
};

const lessonNumber = (lesson: HomeLesson) => {
  const match = lesson.title.match(/Bài\s+(\d+)/iu);
  return match ? Number(match[1]) : lesson.order;
};

const lessonTitle = (lesson: HomeLesson) =>
  lesson.title.replace(/^Bài\s+\d+[.:]\s*/iu, '');

export function HomePage() {
  const navigate = useNavigate();
  const progressTimers = useRef(new Map<number, ReturnType<typeof setTimeout>>());
  const [learningLoading, setLearningLoading] = useState(true);
  const [learningError, setLearningError] = useState('');
  const [searchParams] = useSearchParams();
  const view = searchParams.get('view');
  const activeNav = view === 'lookup' ? 'Tra cứu' : view === 'about' ? 'Về chúng tôi' : 'Lý thuyết';
  const [chapters, setChapters] = useState<HomeChapter[]>([]);
  const [selectedChapterId, setSelectedChapterId] = useState<number | null>(() => readStoredChapterId());
  const [aboutContent, setAboutContent] = useState<AboutContent[]>([]);
  const [aboutLoading, setAboutLoading] = useState(false);
  const [aboutError, setAboutError] = useState('');
  const [chapterMenuOpen, setChapterMenuOpen] = useState(false);
  const [activeLesson, setActiveLesson] = useState<number | null>(null);
  const selectedChapter = chapters.find((chapter) => chapter.id === selectedChapterId) ?? chapters[0];

  const loadChapters = async () => {
    setLearningLoading(true);
    setLearningError('');
    try {
      const data = await getChapters();
      setChapters(data);
      setSelectedChapterId((current) => {
        const preferred = current ?? readStoredChapterId();
        return data.some((chapter) => chapter.id === preferred) ? preferred : data[0]?.id ?? null;
      });
      if (!data.length) setLearningError('Database chưa có bài học.');
    } catch {
      setLearningError('Không thể tải bài học. Kiểm tra API rồi thử lại.');
    } finally {
      setLearningLoading(false);
    }
  };

  useEffect(() => {
    void loadChapters();
    void getProfile()
      .then((profile) => {
        const nextName = profile.fullName?.trim() || profile.username;
        updateAuthenticatedUser({ fullName: nextName, avatar: profile.avatar });
      })
      .catch(() => undefined);

    return () => {
      progressTimers.current.forEach(clearTimeout);
    };
  }, []);

  useEffect(() => {
    if (activeNav !== 'Về chúng tôi' || aboutContent.length || aboutLoading) return;

    setAboutLoading(true);
    setAboutError('');
    void getAboutContent()
      .then(setAboutContent)
      .catch(() => setAboutError('Không thể tải nội dung Về chúng tôi.'))
      .finally(() => setAboutLoading(false));
  }, [activeNav, aboutContent.length, aboutLoading]);

  const patchLessonProgress = (lessonId: number, completionPercent: number, isCompleted: boolean) => {
    setChapters((current) => current.map((chapter) => ({
      ...chapter,
      lessons: chapter.lessons.map((lesson) => lesson.id === lessonId
        ? {
            ...lesson,
            completionPercent: Math.max(lesson.completionPercent ?? 0, completionPercent),
            isCompleted: lesson.isCompleted || isCompleted,
          }
        : lesson),
    })));
  };

  const queueLessonProgress = (lesson: HomeLesson, completionPercent: number, isCompleted = false) => {
    const nextPercent = Math.max(lesson.completionPercent ?? 0, completionPercent);
    const nextCompleted = lesson.isCompleted || isCompleted || nextPercent >= 100;
    patchLessonProgress(lesson.id, nextPercent, nextCompleted);
    clearTimeout(progressTimers.current.get(lesson.id));
    progressTimers.current.set(lesson.id, setTimeout(() => {
      progressTimers.current.delete(lesson.id);
      void saveLessonProgress(lesson.id, nextPercent, nextCompleted)
        .then((saved) => patchLessonProgress(saved.lessonId, saved.completionPercent, saved.isCompleted))
        .catch(() => setLearningError('Không thể lưu tiến độ học. Kiểm tra API rồi thử lại.'));
    }, isCompleted ? 0 : 700));
  };

  return (
    <main className="home-shell">
      <section className="home-learning">
        {activeNav === 'Tra cứu' ? (
          <>
            <PageHeader kicker="Thư viện Hóa học" title="Tra cứu" subtitle="Tìm nguyên tố và hóa chất đang có trong Atomos." />
            <LookupView />
          </>
        ) : activeNav === 'Về chúng tôi' ? (
          <>
            <PageHeader kicker="Atomos" title="Về chúng tôi" subtitle="Atomos giúp học Hóa nhẹ hơn, trực quan hơn và gần với cách học sinh thật sự khám phá." />
            <AboutView items={aboutContent} loading={aboutLoading} error={aboutError} />
          </>
        ) : selectedChapter ? (
          <>
            <PageHeader kicker={`Chương ${selectedChapter.order}`} title={selectedChapter.title.replace(/^Chương\s+\d+[.:]\s*/iu, '')} />

            <div className="chapter-controls">
              <div>
                <span>Chương học</span>
                <h2>{activeNav}</h2>
              </div>
              <div className="chapter-select">
                <ClickAwayListener onClickAway={() => setChapterMenuOpen(false)}>
                  <div className="chapter-select-menu">
                    <button
                      className="chapter-select-button"
                      type="button"
                      aria-expanded={chapterMenuOpen}
                      onClick={() => setChapterMenuOpen((open) => !open)}
                    >
                      <span>Chương {selectedChapter.order}</span>
                      {chapterMenuOpen ? <ExpandLess /> : <ExpandMore />}
                    </button>
                    {chapterMenuOpen && (
                      <div className="chapter-options">
                        {chapters.map((chapter) => (
                          <button
                            className={chapter.id === selectedChapterId ? 'is-selected' : ''}
                            type="button"
                            key={chapter.id}
                            onClick={() => {
                              localStorage.setItem(selectedChapterStorageKey, String(chapter.id));
                              setSelectedChapterId(chapter.id);
                              setActiveLesson(null);
                              setChapterMenuOpen(false);
                            }}
                          >
                            <small>Chương {chapter.order}</small>
                            <strong>{chapter.title.replace(/^Chương\s+\d+[.:]\s*/iu, '')}</strong>
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                </ClickAwayListener>
              </div>
            </div>

            <section className="lesson-list" aria-label={`Bài học ${selectedChapter.title}`}>
              {selectedChapter.lessons.map((lesson) => {
                const number = lessonNumber(lesson);
                const percent = Math.min(100, Math.max(0, lesson.completionPercent ?? 0));
                return (
                  <article
                    className="lesson-card"
                    key={lesson.id}
                  >
                    <button
                      className="lesson-card-summary"
                      type="button"
                      onClick={() => {
                        setActiveLesson(lesson.id);
                        queueLessonProgress(lesson, Math.max(percent, 5));
                        navigate(`/lessons/${lesson.id}`);
                      }}
                    >
                      <div className="lesson-index">{String(number).padStart(2, '0')}</div>
                      <div className="lesson-content">
                        <span className="lesson-label">Bài {number}</span>
                        <h3>{lessonTitle(lesson)}</h3>
                        <div className="lesson-progress-row">
                          <span>Đã học</span>
                          <strong>{lesson.completionPercent ?? 0}%</strong>
                        </div>
                        <LinearProgress
                          className="lesson-progress"
                          variant="determinate"
                          value={percent}
                        />
                      </div>
                    </button>
                  </article>
                );
              })}
            </section>
          </>
        ) : (
          <div className="home-loading" role="status">
            <span>{learningLoading ? 'Đang tải bài học...' : learningError}</span>
            {!learningLoading && learningError && (
              <button type="button" onClick={() => void loadChapters()} aria-label="Tải lại bài học"><RefreshOutlined /></button>
            )}
          </div>
        )}
      </section>

    </main>
  );
}

function AboutView({ items, loading, error }: { items: AboutContent[]; loading: boolean; error: string }) {
  return (
    <section className="about-view">
      {loading && <div className="home-loading"><span>Đang tải nội dung...</span></div>}
      {error && <div className="home-loading is-error"><span>{error}</span></div>}
      {!loading && !error && !items.length && (
        <div className="home-loading"><span>Chưa có nội dung Về chúng tôi.</span></div>
      )}

      <div className="about-list">
        {items.map((item) => (
          <article className={item.imageUrl ? 'about-card has-image' : 'about-card'} key={item.id}>
            <div className="about-card-copy">
              <span>{String(item.order).padStart(2, '0')}</span>
              <h2>{item.title}</h2>
              {hasHtmlContent(item.content) ? (
                <div className="about-rich-content" dangerouslySetInnerHTML={{ __html: sanitizeAboutHtml(item.content) }} />
              ) : (
                item.content.split(/\r?\n/).filter(Boolean).map((line) => (
                  <p key={line}>{line}</p>
                ))
              )}
            </div>
            {item.imageUrl && (
              <img src={resolveAboutAssetUrl(item.imageUrl)} alt={item.title} loading="lazy" />
            )}
          </article>
        ))}
      </div>
    </section>
  );
}

function hasHtmlContent(content: string) {
  return /<[a-z][\s\S]*>/i.test(content);
}

function resolveAboutAssetUrl(url: string) {
  if (/^(data:|https?:\/\/)/i.test(url)) return url;
  if (url.startsWith('/')) return `${homeApiBaseUrl}${url}`;
  return `${homeApiBaseUrl}/${url}`;
}

function sanitizeAboutHtml(content: string) {
  return sanitizeLessonHtml(content, resolveAboutAssetUrl);
}
