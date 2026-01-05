'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import { useState, useEffect } from 'react';
import Select from '@/components/ui/select/Select';

export default function PlayersFilters() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const encounter = searchParams.get('encounter');
  const [characterSearch, setCharacterSearch] = useState(searchParams.get('search') || '');
  const [selectedClass, setSelectedClass] = useState(searchParams.get('class') || '');
  const [selectedRole, setSelectedRole] = useState(searchParams.get('role') || '');

  useEffect(() => {
    setCharacterSearch(searchParams.get('search') || '');
    setSelectedClass(searchParams.get('class') || '');
    setSelectedRole(searchParams.get('role') || '');
  }, [searchParams]);

  const handleFilter = () => {
    const params = new URLSearchParams();
    if (encounter)
      params.append('encounter', encounter);
    if (characterSearch)
      params.append('search', characterSearch);
    if (selectedClass)
      params.append('class', selectedClass);
    if (selectedRole)
      params.append('role', selectedRole);
    
    router.push(`/players?${params.toString()}`);
  };

  const handleReset = () => {
    setCharacterSearch('');
    setSelectedClass('');
    setSelectedRole('');
    if (encounter)
      router.push(`/players?encounter=${encodeURIComponent(encounter)}`);
    else
      router.push('/players');
  };

  return (
    <div className="p-4">
      <h2 className="text-xs font-semibold text-[#9ca3af] uppercase tracking-wider mb-4">Фильтры</h2>
        
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-medium text-[#9ca3af] mb-2">
              Игрок или GUID
            </label>
            <input
              type="text"
              value={characterSearch}
              onChange={(e) => setCharacterSearch(e.target.value)}
              className="w-full bg-[#1e1e1e] border border-[#333333] rounded px-3 py-2 text-[#e5e5e5] text-sm focus:outline-none focus:border-[#4a4a4a]"
              placeholder="Поиск..."
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-[#9ca3af] mb-2">
              Класс
            </label>
            <Select
              value={selectedClass}
              onChange={setSelectedClass}
              placeholder="Все классы"
              options={[
                { value: '', label: 'Все классы' },
                { value: '1', label: 'Воин' },
                { value: '2', label: 'Паладин' },
                { value: '3', label: 'Охотник' },
                { value: '4', label: 'Разбойник' },
                { value: '5', label: 'Жрец' },
                { value: '6', label: 'Рыцарь смерти' },
                { value: '7', label: 'Шаман' },
                { value: '8', label: 'Маг' },
                { value: '9', label: 'Чернокнижник' },
                { value: '11', label: 'Друид' },
              ]}
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-[#9ca3af] mb-2">
              Роль
            </label>
            <Select
              value={selectedRole}
              onChange={setSelectedRole}
              placeholder="Все роли"
              options={[
                { value: '', label: 'Все роли' },
                { value: '1', label: 'Танк' },
                { value: '2', label: 'Лечение' },
                { value: '0', label: 'Урон' },
              ]}
            />
          </div>

          <div className="pt-4 border-t border-[#333333]">
            <button
              onClick={handleFilter}
              className="w-full bg-white text-black rounded px-3 py-2 text-sm font-semibold hover:bg-[#e5e5e5] transition-colors mb-2"
            >
              Применить фильтры
            </button>
            <button
              onClick={handleReset}
              className="w-full bg-transparent border border-[#333333] text-[#9ca3af] rounded px-3 py-2 text-sm font-medium hover:bg-[#2d2d2d] hover:text-[#e5e5e5] transition-colors"
            >
              Сбросить фильтры
            </button>
          </div>
        </div>
    </div>
  );
}

