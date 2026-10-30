import {
  BiotechOutlined,
  Close,
  GridViewOutlined,
  SearchOutlined,
} from '@mui/icons-material';
import { useEffect, useMemo, useRef, useState } from 'react';
import {
  getLookupChemicals,
  getLookupElements,
  LookupChemical,
  LookupElement,
} from './homeApi';
import './lookup.css';

type LookupTab = 'elements' | 'chemicals';
type ChemicalCategory = 'Tất cả' | 'Kim loại' | 'Phi kim' | 'Acid' | 'Bazơ' | 'Muối' |
  'Oxit acid' | 'Oxit bazơ' | 'Oxit lưỡng tính' | 'Oxit trung tính';

const chemicalCategories: ChemicalCategory[] = [
  'Tất cả',
  'Kim loại',
  'Phi kim',
  'Acid',
  'Bazơ',
  'Muối',
  'Oxit acid',
  'Oxit bazơ',
  'Oxit lưỡng tính',
  'Oxit trung tính',
];

const normalize = (value?: string | null) =>
  (value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();

const elementTone = (type?: string | null) => {
  const value = normalize(type);
  if (value.includes('kiem')) return 'alkali';
  if (value.includes('halogen')) return 'halogen';
  if (value.includes('khi hiem')) return 'noble';
  if (value.includes('a kim')) return 'metalloid';
  if (value.includes('phi kim')) return 'nonmetal';
  return 'metal';
};

const oxideCategory = (chemical: LookupChemical): ChemicalCategory | null => {
  if (!normalize(chemical.type).includes('ox')) return null;

  const detail = normalize([
    chemical.type,
    chemical.name,
    chemical.properties,
    chemical.description,
    chemical.phenomenon,
  ].join(' '));
  if (detail.includes('luong tinh')) return 'Oxit lưỡng tính';
  if (detail.includes('trung tinh')) return 'Oxit trung tính';
  if (detail.includes('oxit acid') || detail.includes('oxide acid')) return 'Oxit acid';
  if (detail.includes('oxit base') || detail.includes('oxide base') || detail.includes('oxit bazo')) return 'Oxit bazơ';

  if (['Al2O3', 'ZnO'].includes(chemical.formula)) return 'Oxit lưỡng tính';
  if (['CO', 'NO', 'N2O'].includes(chemical.formula)) return 'Oxit trung tính';
  if (['CO2', 'SO2', 'SO3', 'NO2', 'N2O5', 'P2O5', 'P2O3', 'SiO2'].includes(chemical.formula)) return 'Oxit acid';
  return 'Oxit bazơ';
};

const chemicalCategory = (chemical: LookupChemical): ChemicalCategory => {
  const oxide = oxideCategory(chemical);
  if (oxide) return oxide;

  const type = normalize(chemical.type);
  if (type.includes('kim loai')) return 'Kim loại';
  if (type.includes('phi kim')) return 'Phi kim';
  if (type.includes('acid') || type.includes('axit')) return 'Acid';
  if (type.includes('base') || type.includes('bazo')) return 'Bazơ';
  if (type.includes('muoi')) return 'Muối';
  return 'Tất cả';
};

export function LookupView() {
  const [tab, setTab] = useState<LookupTab>('elements');
  const [query, setQuery] = useState('');
  const [elements, setElements] = useState<LookupElement[]>([]);
  const [chemicals, setChemicals] = useState<LookupChemical[]>([]);
  const [selectedElement, setSelectedElement] = useState<LookupElement | null>(null);
  const [selectedChemical, setSelectedChemical] = useState<LookupChemical | null>(null);
  const [selectedChemicalCategory, setSelectedChemicalCategory] = useState<ChemicalCategory>('Tất cả');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const elementDialogRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    Promise.all([getLookupElements(), getLookupChemicals()])
      .then(([elementRows, chemicalRows]) => {
        if (cancelled) return;
        setElements(elementRows);
        setChemicals(chemicalRows);
        setSelectedChemical(chemicalRows[0] ?? null);
      })
      .catch(() => !cancelled && setError('Không thể tải dữ liệu tra cứu.'))
      .finally(() => !cancelled && setLoading(false));
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    if (!selectedElement || tab !== 'elements') return undefined;

    const closeOnOutsideClick = (event: PointerEvent) => {
      const target = event.target;
      if (!(target instanceof Node)) return;
      if (elementDialogRef.current?.contains(target)) return;
      setSelectedElement(null);
    };

    document.addEventListener('pointerdown', closeOnOutsideClick);
    return () => document.removeEventListener('pointerdown', closeOnOutsideClick);
  }, [selectedElement, tab]);

  const filteredElements = useMemo(() => {
    const value = normalize(query.trim());
    if (!value) return elements;
    return elements.filter((element) => normalize(
      `${element.atomicNumber} ${element.symbol} ${element.name} ${element.englishName} ${element.type}`,
    ).includes(value));
  }, [elements, query]);

  const filteredChemicals = useMemo(() => {
    const value = normalize(query.trim());
    return chemicals.filter((chemical) => {
      const matchesCategory = selectedChemicalCategory === 'Tất cả' ||
        chemicalCategory(chemical) === selectedChemicalCategory;
      const matchesQuery = !value || normalize(
        `${chemical.formula} ${chemical.name} ${chemical.type} ${chemical.state} ${chemical.color}`,
      ).includes(value);
      return matchesCategory && matchesQuery;
    });
  }, [chemicals, query, selectedChemicalCategory]);

  if (loading) return <div className="lookup-state">Đang tải dữ liệu tra cứu...</div>;
  if (error) return <div className="lookup-state is-error">{error}</div>;

  return (
    <section className="lookup-view">
      <div className="lookup-toolbar">
        <div className="lookup-tabs" role="tablist" aria-label="Loại dữ liệu tra cứu">
          <button className={tab === 'elements' ? 'is-active' : ''} type="button" onClick={() => { setTab('elements'); setQuery(''); }}>
            <GridViewOutlined /> Bảng tuần hoàn
          </button>
          <button className={tab === 'chemicals' ? 'is-active' : ''} type="button" onClick={() => { setTab('chemicals'); setQuery(''); setSelectedElement(null); }}>
            <BiotechOutlined /> Hóa chất
          </button>
        </div>
        <label className="lookup-search">
          <SearchOutlined />
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder={tab === 'elements' ? 'Tìm tên, ký hiệu, số hiệu...' : 'Tìm tên hoặc công thức...'}
          />
          {query && <button type="button" aria-label="Xóa tìm kiếm" onClick={() => setQuery('')}><Close /></button>}
        </label>
      </div>

      {tab === 'elements' ? (
        <div className="periodic-layout">
          <div className="periodic-board">
            <div className="periodic-grid" aria-label="Bảng tuần hoàn các nguyên tố">
              {filteredElements.map((element) => (
                <button
                  type="button"
                  key={element.id}
                  className={`element-tile is-${elementTone(element.type)} ${selectedElement?.id === element.id ? 'is-selected' : ''}`}
                  style={{ gridColumn: element.group ?? 1, gridRow: element.period ?? 1 }}
                  onClick={() => setSelectedElement(element)}
                  aria-label={`${element.name} (${element.symbol})`}
                >
                  <small>{element.atomicNumber}</small>
                  <strong>{element.symbol}</strong>
                  <span>{element.name}</span>
                </button>
              ))}
            </div>
            {selectedElement && (
              <div className="element-detail-overlay" onClick={() => setSelectedElement(null)}>
                <div ref={elementDialogRef} className="element-detail-dialog" onClick={(event) => event.stopPropagation()}>
                  <ElementDetails element={selectedElement} />
                </div>
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className="chemical-section">
          <div className="chemical-catalog">
            <label htmlFor="chemical-category">Danh mục</label>
            <select
              id="chemical-category"
              value={selectedChemicalCategory}
              onChange={(event) => {
                const category = event.target.value as ChemicalCategory;
                setSelectedChemicalCategory(category);
                setSelectedChemical(chemicals.find((chemical) =>
                  category === 'Tất cả' || chemicalCategory(chemical) === category) ?? null);
              }}
            >
              {chemicalCategories.map((category) => <option key={category}>{category}</option>)}
            </select>
            <span>{filteredChemicals.length} hóa chất</span>
          </div>
          <div className="chemical-layout">
            <div className="chemical-results" aria-label="Danh sách hóa chất">
            {filteredChemicals.map((chemical) => (
              <button
                type="button"
                key={chemical.id}
                className={selectedChemical?.id === chemical.id ? 'chemical-row is-selected' : 'chemical-row'}
                onClick={() => setSelectedChemical(chemical)}
              >
                <strong>{chemical.formula}</strong>
                <span>{chemical.name}</span>
                <small>{chemicalCategory(chemical) === 'Tất cả' ? 'Chưa phân loại' : chemicalCategory(chemical)}</small>
              </button>
            ))}
            {!filteredChemicals.length && <p className="lookup-empty">Không tìm thấy hóa chất phù hợp.</p>}
            </div>
            <ChemicalDetails chemical={selectedChemical} />
          </div>
        </div>
      )}
    </section>
  );
}

function ElementDetails({ element }: { element: LookupElement | null }) {
  if (!element) return <aside className="lookup-detail"><p>Chọn một nguyên tố để xem chi tiết.</p></aside>;
  return (
    <aside className="lookup-detail element-detail">
      <header>
        <div className={`element-symbol is-${elementTone(element.type)}`}><small>{element.atomicNumber}</small>{element.symbol}</div>
        <div><span>{element.type || 'Nguyên tố'}</span><h2>{element.name}</h2><p>{element.englishName}</p></div>
      </header>
      <dl>
        <div><dt>Nguyên tử khối</dt><dd>{element.atomicMass ?? 'Chưa có'}</dd></div>
        <div><dt>Chu kỳ / Nhóm</dt><dd>{element.period ?? '-'} / {element.group ?? '-'}</dd></div>
        <div><dt>Trạng thái</dt><dd>{element.state || 'Chưa có'}</dd></div>
        <div><dt>Độ âm điện</dt><dd>{element.electronegativity ?? 'Chưa có'}</dd></div>
        <div className="is-wide"><dt>Cấu hình electron</dt><dd>{element.electronConfiguration || 'Chưa có'}</dd></div>
      </dl>
      {element.description && <section><h3>Đặc điểm</h3><p>{element.description}</p></section>}
      {element.applications && <section><h3>Ứng dụng</h3><p>{element.applications}</p></section>}
    </aside>
  );
}

function ChemicalDetails({ chemical }: { chemical: LookupChemical | null }) {
  if (!chemical) return <aside className="lookup-detail"><p>Chọn một hóa chất để xem chi tiết.</p></aside>;
  return (
    <aside className="lookup-detail chemical-detail">
      <header><span>{chemicalCategory(chemical) === 'Tất cả' ? 'Hóa chất' : chemicalCategory(chemical)}</span><h2>{chemical.formula}</h2><p>{chemical.name}</p></header>
      <dl>
        <div><dt>Khối lượng mol</dt><dd>{chemical.molarMass ? `${chemical.molarMass} g/mol` : 'Chưa có'}</dd></div>
        <div><dt>Trạng thái</dt><dd>{chemical.state || 'Chưa có'}</dd></div>
        <div><dt>Màu sắc</dt><dd>{chemical.color || 'Chưa có'}</dd></div>
        <div><dt>Độ tan</dt><dd>{chemical.solubility || 'Chưa có'}</dd></div>
        {chemical.defaultPh != null && <div><dt>pH mặc định</dt><dd>{chemical.defaultPh}</dd></div>}
      </dl>
      {(chemical.description || chemical.properties) && <section><h3>Đặc điểm</h3><p>{chemical.description || chemical.properties}</p></section>}
      {chemical.applications && <section><h3>Ứng dụng</h3><p>{chemical.applications}</p></section>}
      {chemical.phenomenon && <section><h3>Trong phản ứng</h3><p>{chemical.phenomenon}</p></section>}
    </aside>
  );
}
