import Link from 'next/link';
import Image from 'next/image';
import { getEncounterImage } from '@/lib/encounterImages';

interface EncounterSidebarProps {
  encounters: Array<{ encounterEntry: string; encounterName: string | null }>;
  selectedEncounter?: string;
}

export default function EncounterSidebar({ encounters, selectedEncounter }: EncounterSidebarProps) {
  const isAllSelected = !selectedEncounter;
  
  return (
    <div className="p-4" suppressHydrationWarning>
      <h2 className="text-xs font-semibold text-[#9ca3af] uppercase tracking-wider mb-3 px-2">Боссы</h2>
      <nav className="space-y-0.5">
        <Link
          href="/players"
          className={isAllSelected
            ? 'flex items-center gap-3 px-3 py-3 rounded text-base font-medium transition-colors bg-[#3a3a3a] text-[#e5e5e5]'
            : 'flex items-center gap-3 px-3 py-3 rounded text-base font-medium transition-colors text-[#9ca3af] hover:text-[#e5e5e5] hover:bg-[#2d2d2d]'}
        >
          <span className="w-12 h-12 bg-[#3a3a3a] rounded flex-shrink-0"></span>
          <span>Все игроки</span>
        </Link>
        {encounters.map((encounter) => {
          const encounterName = encounter.encounterName ?? encounter.encounterEntry;
          const imagePath = getEncounterImage(encounter.encounterName);
          const isSelected = selectedEncounter === encounter.encounterName || selectedEncounter === encounter.encounterEntry;
          const encodedName = encodeURIComponent(encounterName);
          
          return (
            <Link
              key={encounter.encounterEntry}
              href={`/players?encounter=${encodedName}`}
              className={isSelected
                ? 'flex items-center gap-3 px-3 py-3 rounded text-base font-medium transition-colors bg-[#3a3a3a] text-[#e5e5e5]'
                : 'flex items-center gap-3 px-3 py-3 rounded text-base font-medium transition-colors text-[#9ca3af] hover:text-[#e5e5e5] hover:bg-[#2d2d2d]'}
            >
              {imagePath ? (
                <Image
                  src={imagePath}
                  alt={encounterName}
                  width={48}
                  height={48}
                  className="w-12 h-12 rounded flex-shrink-0 object-cover"
                  unoptimized
                />
              ) : (
                <span className="w-12 h-12 bg-[#3a3a3a] rounded flex-shrink-0"></span>
              )}
              <span>{encounterName}</span>
            </Link>
          );
        })}
      </nav>
    </div>
  );
}

