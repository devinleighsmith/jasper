/**
 * Formats a given date string into the format "DD-MMM-YYYY".
 *
 * @param dateString - The date string to format. It should be a valid date string.
 * @returns {string} A formatted date string in "DD-MMM-YYYY" format (e.g., "01-Jan-2023").
 */
export const formatDateToDDMMMYYYY = (dateString: string): string => {
  const date = dateString ? new Date(dateString) : null;
  if (!date) {
    return '';
  }

  const day = new Intl.DateTimeFormat('en', { day: '2-digit' }).format(date);
  const month = new Intl.DateTimeFormat('en', { month: 'short' }).format(date);
  const year = new Intl.DateTimeFormat('en', { year: 'numeric' }).format(date);

  return `${day}-${month}-${year}`;
};

export const hoursMinsFormatter = (hours: string, minutes: string) => {
  const hrs = parseInt(hours, 10);
  const mins = parseInt(minutes, 10);
  let result = '';
  if (hrs) {
    result += `${hrs} Hr(s)`;
  }
  if (mins) {
    result += `${result ? ' ' : ''}${mins} Min(s)`;
  }
  return result || '0 Mins';
};

export const extractTime = (date: string) => {
    const time = date.split(' ')[1];
    const hours = parseInt(time.slice(0, 2), 10);
    const minutes = time.slice(3, 5);
    const period = hours >= 12 ? 'PM' : 'AM';
    const formattedHours = hours % 12 || 12; // Convert to 12-hour format
    return `${formattedHours}:${minutes} ${period}`;
  };